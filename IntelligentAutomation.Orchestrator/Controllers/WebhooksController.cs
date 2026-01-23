using IntelligentAutomation.Domain.Enums;
using IntelligentAutomation.Domain.Workflow;
using IntelligentAutomation.Infrastructure.Persistence;
using IntelligentAutomation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Text.Json;

namespace IntelligentAutomation.Orchestrator.Controllers;

[ApiController]
[Route("[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly MongoDbContext _db;
    private readonly IContainerManagerService _containerManager;
    private readonly IAgentLogService _logService;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        MongoDbContext db,
        IContainerManagerService containerManager,
        IAgentLogService logService,
        ILogger<WebhooksController> logger)
    {
        _db = db;
        _containerManager = containerManager;
        _logService = logService;
        _logger = logger;
    }

    [HttpPost("v1/trigger/{agentId}")]
    public async Task<IActionResult> TriggerAgent(Guid agentId, [FromQuery] string secret)
    {
        var agent = await _db.Agents.Find(a => a.Id == agentId).FirstOrDefaultAsync();

        if (agent == null)
        {
            return NotFound(new { Message = "Agente não encontrado." });
        }

        var definition = JsonSerializer.Deserialize<WorkflowDefinition>(agent.DefinitionJson);
        if (definition?.Trigger == null || definition.Trigger.Type != TriggerType.Webhook)
        {
            return BadRequest(new { Message = "Este agente não está configurado para gatilhos via Webhook." });
        }

        if (definition.Trigger.WebhookSecret != secret)
        {
            _logger.LogWarning("Tentativa de trigger inválida para Agente {AgentId} com secret incorreto.", agentId);
            return Unauthorized(new { Message = "Secret inválido." });
        }

        try
        {
            _logger.LogInformation("Webhook recebido para o Agente {AgentId}. Iniciando execução.", agentId);

            await _containerManager.StartAgentAsync(agentId, default);

            await _logService.LogAsync(agentId.ToString(), "Execução iniciada via Webhook externo.", "Information", "Webhook");

            return Ok(new { Status = "Triggered", AgentId = agentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar trigger de webhook para o agente {AgentId}", agentId);
            return StatusCode(500, "Erro interno ao processar o gatilho.");
        }
    }
}
