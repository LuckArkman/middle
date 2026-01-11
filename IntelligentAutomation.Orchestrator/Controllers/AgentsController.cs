using System.Text.Json;
using IntelligentAutomation.Application.Dtos;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Domain.Enums;
using IntelligentAutomation.Domain.Workflow;
using IntelligentAutomation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
// ... (outros usings necessários)

namespace IntelligentAutomation.Orchestrator.Controllers;

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
    public async Task<IActionResult> GetAgentById(string id)
    {
        var agent = await _agentsCollection.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();
        return Ok(agent);
    }

    [HttpPut("{id}/definition")]
    public async Task<IActionResult> UpdateAgentDefinition(string id, [FromBody] WorkflowDefinition definition)
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
}