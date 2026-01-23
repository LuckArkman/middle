using System.Text.Json;
using IntelligentAutomation.Dtos;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Domain.Enums;
using IntelligentAutomation.Infrastructure.Persistence;
using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Domain.Workflow;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace IntelligentAutomation.Orchestrator.Controllers;

[ApiController]
[Route("[controller]")]
public class AgentsController : ControllerBase
{
    private readonly MongoDbContext _db;
    private readonly IAgentSchedulingService _schedulingService;
    private readonly IAgentLogService _logService;
    private readonly IContainerManagerService _containerManager;

    public AgentsController(
        MongoDbContext db,
        IAgentSchedulingService schedulingService,
        IAgentLogService logService,
        IContainerManagerService containerManager)
    {
        _db = db;
        _schedulingService = schedulingService;
        _logService = logService;
        _containerManager = containerManager;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentDto dto)
    {
        var agent = new Agent
        {
            Name = dto.Name,
            DefinitionJson = dto.DefinitionJson,
            Status = AgentStatus.Created,
            UserId = User.Identity?.Name ?? "system"
        };

        await _db.Agents.InsertOneAsync(agent);

        await _logService.LogAsync(agent.Id.ToString(), "Agente criado com sucesso.", "Information", "Lifecycle");

        return CreatedAtAction(nameof(GetAgentById), new { id = agent.Id }, agent);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAgentById(Guid id)
    {
        var agent = await _db.Agents.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();
        return Ok(agent);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartAgent(Guid id)
    {
        var agent = await _db.Agents.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();

        try
        {
            await _containerManager.StartAgentAsync(agent.Id, default);

            var update = Builders<Agent>.Update.Set(a => a.Status, AgentStatus.Running);
            await _db.Agents.UpdateOneAsync(a => a.Id == id, update);

            await _logService.LogAsync(id.ToString(), "Comando de início enviado ao Container Manager.", "Information", "Execution");
            return Ok(new { Status = "Started" });
        }
        catch (Exception ex)
        {
            await _logService.LogAsync(id.ToString(), $"Falha ao iniciar agente: {ex.Message}", "Error", "Execution");
            return StatusCode(500, "Erro ao iniciar agente.");
        }
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> StopAgent(Guid id)
    {
        var agent = await _db.Agents.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();

        await _containerManager.StopAgentAsync(agent.Id, default);

        var update = Builders<Agent>.Update.Set(a => a.Status, AgentStatus.Stopped);
        await _db.Agents.UpdateOneAsync(a => a.Id == id, update);

        await _logService.LogAsync(id.ToString(), "Agente parado manualmente.", "Warning", "Execution");
        return Ok(new { Status = "Stopped" });
    }

    [HttpGet("{id}/logs")]
    public async Task<IActionResult> GetAgentLogs(Guid id, [FromQuery] int limit = 100)
    {
        var logs = await _logService.GetLogsAsync(id.ToString(), limit);
        return Ok(logs);
    }

    [HttpPut("{id}/definition")]
    public async Task<IActionResult> UpdateAgentDefinition(Guid id, [FromBody] WorkflowDefinition definition)
    {
        var agent = await _db.Agents.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();

        var update = Builders<Agent>.Update
            .Set(a => a.DefinitionJson, JsonSerializer.Serialize(definition))
            .Set(a => a.UpdatedAt, DateTime.UtcNow);

        await _db.Agents.UpdateOneAsync(a => a.Id == id, update);

        // Lógica de agendamento se o trigger for do tipo Schedule (Cron)
        if (definition.Trigger != null && definition.Trigger.Type == TriggerType.Cron)
        {
            var cron = definition.Trigger.CronExpression;
            if (!string.IsNullOrEmpty(cron))
            {
                await _schedulingService.ScheduleOrUpdateAgentJob(id, cron);
                await _logService.LogAsync(id.ToString(), $"Agente reagendado com cron: {cron}", "Information", "Scheduling");
            }
        }
        else
        {
            await _schedulingService.UnscheduleAgentJob(id);
        }

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetAgents([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var findOptions = new FindOptions<Agent>
        {
            Limit = pageSize,
            Skip = (page - 1) * pageSize,
            Sort = Builders<Agent>.Sort.Descending(a => a.CreatedAt)
        };

        var total = await _db.Agents.CountDocumentsAsync(_ => true);
        var items = await _db.Agents.Find(_ => true).Limit(pageSize).Skip((page - 1) * pageSize).SortByDescending(a => a.CreatedAt).ToListAsync();

        return Ok(new { Items = items, Total = total });
    }

    [HttpGet("{id}/definition")]
    public async Task<IActionResult> GetAgentDefinition(Guid id)
    {
        var agent = await _db.Agents.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();
        return Ok(agent.DefinitionJson);
    }

    [HttpPost("{id}/logs")]
    public async Task<IActionResult> AddAgentLog(Guid id, [FromBody] LogRequest dto)
    {
        await _logService.LogAsync(id.ToString(), dto.Message, dto.Level, dto.Category, dto.Details);
        return Ok();
    }
}

public class LogRequest
{
    public string Message { get; set; } = string.Empty;
    public string Level { get; set; } = "Information";
    public string Category { get; set; } = "Execution";
    public Dictionary<string, object>? Details { get; set; }
}