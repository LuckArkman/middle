using System.Text.Json;
using System.Text.Json.Serialization;
using IntelligentAutomation.Application.Dtos;
using IntelligentAutomation.Application.Services;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Domain.Workflow;
using IntelligentAutomation.Infrastructure.Persistence;
using IntelligentAutomation.WebApp.Components.Pages;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentAutomation.Orchestrator.Controllers;

[ApiController]
[Route("[controller]")]
public class AgentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AgentsController> _logger;
    private readonly IContainerManagerService _containerManagerService;

    public AgentsController(
        ApplicationDbContext context, 
        ILogger<AgentsController> logger, 
        IContainerManagerService containerManagerService)
    {
        _context = context;
        _logger = logger;
        _containerManagerService = containerManagerService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentDto dto)
    {
        _logger.LogInformation("Recebida requisição para criar um novo agente com nome: {AgentName}", dto.Name);

        var agent = new Agent
        {
            Name = dto.Name,
            DefinitionJson = dto.DefinitionJson,
            Status = AgentStatus.Created,
            UserId = Guid.NewGuid() // Mock: Associar ao usuário logado no futuro
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();
        
        // Solicita o provisionamento do container de forma assíncrona
        await _containerManagerService.ProvisionAgentAsync(new ProvisionAgentRequest
        {
            AgentId = agent.Id,
            AgentName = agent.Name
        });

        return CreatedAtAction(nameof(GetAgentById), new { id = agent.Id }, agent);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAgentById(Guid id)
    {
        var agent = await _context.Agents.FindAsync(id);
        if (agent == null) return NotFound();
        return Ok(agent);
    }

    [HttpPost("{id}/execute")]
    public async Task<IActionResult> ExecuteAgent(Guid id)
    {
        var agent = await _context.Agents.FindAsync(id);
        if (agent == null) return NotFound();

        agent.Status = AgentStatus.Running;
        await _context.SaveChangesAsync();
        
        await _containerManagerService.StartAgentAsync(id);
        _logger.LogInformation("Agente {AgentId} teve seu status alterado para Running.", id);

        return Ok($"Agente {id} foi iniciado com sucesso.");
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> StopAgent(Guid id)
    {
        var agent = await _context.Agents.FindAsync(id);
        if (agent == null) return NotFound();

        agent.Status = AgentStatus.Stopped;
        await _context.SaveChangesAsync();

        await _containerManagerService.StopAgentAsync(id);
        _logger.LogInformation("Agente {AgentId} teve seu status alterado para Stopped.", id);

        return Ok($"Agente {id} foi parado com sucesso.");
    }
    
    [HttpPut("{id}/definition")]
    public async Task<IActionResult> UpdateAgentDefinition(Guid id, [FromBody] WorkflowDefinition definition)
    {
        var agent = await _context.Agents.FindAsync(id);
        if (agent == null) return NotFound();

        _logger.LogInformation("Atualizando a definição do Agente ID: {AgentId}", id);
    
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            TypeInfoResolver = new AgentBuilder.PolymorphicTypeResolver() // Usa nosso resolvedor
        };

        agent.DefinitionJson = JsonSerializer.Serialize(definition, jsonOptions);
        agent.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}