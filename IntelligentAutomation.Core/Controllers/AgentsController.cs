using System.Text.Json;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Domain.Enums;
using IntelligentAutomation.Domain.Workflow;
using IntelligentAutomation.Dtos;
using IntelligentAutomation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
// ... (outros usings necessários)

namespace IntelligentAutomation.Core.Controllers;

[ApiController]
[Route("[controller]")]
public class AgentsController : ControllerBase
{
    private readonly IMongoCollection<Agent> _agentsCollection;

    public AgentsController(
        MongoDbContext mongoContext
        )
    {
        _agentsCollection = mongoContext.Agents;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentDto dto)
    {
        // A lógica de QuotaService precisará ser refatorada também, mas vamos focar no controller primeiro
        var agent = new Agent
        {
            Name = dto.Name,
            DefinitionJson = dto.DefinitionJson,
            Status = AgentStatus.Created,
            UserId = Guid.NewGuid().ToString() // Mock
        };

        await _agentsCollection.InsertOneAsync(agent);

        // ... (chamar o containerManagerService)

        return CreatedAtAction(nameof(GetAgentById), new { id = agent.Id }, agent);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAgentById(Guid id)
    {
        var agent = await _agentsCollection.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();
        return Ok(agent);
    }

    [HttpPut("{id}/definition")]
    public async Task<IActionResult> UpdateAgentDefinition(Guid id, [FromBody] WorkflowDefinition definition)
    {
        var agent = await _agentsCollection.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();

        // A lógica de serialização JSON permanece a mesma
        agent.DefinitionJson = JsonSerializer.Serialize(definition, GetJsonOptions());
        agent.UpdatedAt = DateTime.UtcNow;

        await _agentsCollection.ReplaceOneAsync(a => a.Id == id, agent);

        // A lógica de agendamento com Quartz permanece a mesma
        // ...

        return NoContent();
    }

    // ... (outros métodos como Execute/Stop precisariam de uma refatoração similar)

    private JsonSerializerOptions GetJsonOptions()
    {
        // Retorna as opções com o PolymorphicTypeResolver, como definido anteriormente
        return new JsonSerializerOptions { /* ... */ };
    }

    [HttpGet]
    public async Task<IActionResult> GetAgents(
        [FromQuery] string? searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = "obter_id_do_token_jwt"; // Obter o UserId real do token

        var filter = Builders<Agent>.Filter.Eq(a => a.UserId, userId);
        if (!string.IsNullOrEmpty(searchTerm))
        {
            filter &= Builders<Agent>.Filter.Regex(a => a.Name, new BsonRegularExpression(searchTerm, "i"));
        }

        var totalCount = await _agentsCollection.CountDocumentsAsync(filter);
        var agents = await _agentsCollection.Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        // Mapear para DTOs
        var agentDtos = agents.Select(a => new AgentDto { /* ... */ }).ToList();

        return Ok(new PagedResult<AgentDto> { Items = agentDtos, TotalCount = (int)totalCount });
    }
}