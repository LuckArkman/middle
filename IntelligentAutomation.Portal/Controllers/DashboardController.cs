using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Domain.Enums;
using IntelligentAutomation.Enums;
using IntelligentAutomation.Infrastructure.Persistence;
using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Domain.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Text.Json;
using System.Security.Claims;

namespace IntelligentAutomation.Portal.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly MongoDbContext _db;
    private readonly IAgentLogService _logService;
    private readonly IContainerManagerService _containerManager;
    private readonly IAgentSchedulingService _schedulingService;
    private readonly IQuotaService _quotaService;
    private readonly IConfiguration _config;

    public DashboardController(
        MongoDbContext db,
        IAgentLogService logService,
        IContainerManagerService containerManager,
        IAgentSchedulingService schedulingService,
        IQuotaService quotaService,
        IConfiguration config)
    {
        _db = db;
        _logService = logService;
        _containerManager = containerManager;
        _schedulingService = schedulingService;
        _quotaService = quotaService;
        _config = config;
    }

    public async Task<IActionResult> Index()
    {
        var tenantId = User.FindFirstValue("TenantId");
        var agents = await _db.Agents.Find(a => a.TenantId == tenantId).SortByDescending(a => a.CreatedAt).ToListAsync();

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdStr, out var userId))
        {
            var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null && !string.IsNullOrEmpty(user.CurrentSubscriptionId) && Guid.TryParse(user.CurrentSubscriptionId, out var subId))
            {
                var sub = await _db.Subscriptions.Find(s => s.Id == subId).FirstOrDefaultAsync();
                if (sub != null)
                {
                    var plan = await _db.Plans.Find(p => p.Id == sub.PlanId).FirstOrDefaultAsync();
                    ViewBag.PlanName = plan?.Name ?? "Nenhum";
                }
                else
                {
                    ViewBag.PlanName = "Nenhum";
                }
            }
            else
            {
                ViewBag.PlanName = "Nenhum";
            }
        }

        return View(agents);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(Agent agent, string triggerType)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        var tenantId = User.FindFirstValue("TenantId") ?? "system";

        var quota = await _quotaService.CheckAgentCreationQuotaAsync(userId);
        if (quota != QuotaCheckResult.Allowed)
        {
            TempData["Error"] = quota == QuotaCheckResult.MaxAgentsReached
                ? "Você atingiu o limite de agentes do seu plano atual."
                : "Assinatura ativa não encontrada. Por favor, assine um plano.";
            return RedirectToAction(nameof(Index));
        }

        agent.Status = AgentStatus.Created;
        agent.UserId = userId;
        agent.TenantId = tenantId;

        // Gera definição básica baseada no gatilho
        var definition = new WorkflowDefinition
        {
            Nodes = new List<BaseNode>(),
            Connections = new List<Connection>()
        };

        if (triggerType == "Webhook")
        {
            definition.Trigger = new TriggerNode
            {
                Type = TriggerType.Webhook,
                WebhookSecret = Guid.NewGuid().ToString("N").Substring(0, 16)
            };
        }
        else if (triggerType == "Cron")
        {
            definition.Trigger = new TriggerNode
            {
                Type = TriggerType.Cron,
                CronExpression = "0 0 * * * ?" // Diário às 00:00 como padrão
            };
        }

        agent.DefinitionJson = JsonSerializer.Serialize(definition);

        await _db.Agents.InsertOneAsync(agent);

        await _logService.LogAsync(agent.Id.ToString(), "Agente criado via Portal.", "Information", "Lifecycle");

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var agent = await _db.Agents.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();

        ViewBag.Logs = await _logService.GetLogsAsync(id.ToString(), 50);

        if (!string.IsNullOrEmpty(agent.DefinitionJson))
        {
            ViewBag.Definition = JsonSerializer.Deserialize<WorkflowDefinition>(agent.DefinitionJson);
        }

        ViewBag.WebhookBaseUrl = _config["Services:OrchestratorUrl"] ?? "http://localhost:5001";

        return View(agent);
    }

    [HttpPost]
    public async Task<IActionResult> Start(Guid id)
    {
        var agent = await _db.Agents.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();

        try
        {
            await _containerManager.StartAgentAsync(agent.Id, default);

            var update = Builders<Agent>.Update.Set(a => a.Status, AgentStatus.Running);
            await _db.Agents.UpdateOneAsync(a => a.Id == id, update);

            await _logService.LogAsync(id.ToString(), "Execução iniciada pelo usuário.", "Information", "Execution");
        }
        catch (Exception ex)
        {
            await _logService.LogAsync(id.ToString(), $"Erro ao iniciar: {ex.Message}", "Error", "Execution");
            TempData["Error"] = "Não foi possível iniciar o agente. Verifique se o Docker está rodando.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Stop(Guid id)
    {
        var agent = await _db.Agents.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();

        await _containerManager.StopAgentAsync(agent.Id, default);

        var update = Builders<Agent>.Update.Set(a => a.Status, AgentStatus.Stopped);
        await _db.Agents.UpdateOneAsync(a => a.Id == id, update);

        await _logService.LogAsync(id.ToString(), "Execução interrompida pelo usuário.", "Warning", "Execution");

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> EditWorkflow(Guid id)
    {
        var agent = await _db.Agents.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();

        var definition = JsonSerializer.Deserialize<WorkflowDefinition>(agent.DefinitionJson) ?? new WorkflowDefinition();

        ViewBag.AgentId = id;
        ViewBag.AgentName = agent.Name;

        return View(definition);
    }

    [HttpPost]
    public async Task<IActionResult> SaveWorkflow(Guid id, [FromBody] WorkflowDefinition definition)
    {
        var agent = await _db.Agents.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();

        var update = Builders<Agent>.Update
            .Set(a => a.DefinitionJson, JsonSerializer.Serialize(definition))
            .Set(a => a.UpdatedAt, DateTime.UtcNow);

        await _db.Agents.UpdateOneAsync(a => a.Id == id, update);

        // Reagendamento se necessário
        if (definition.Trigger != null && definition.Trigger.Type == TriggerType.Cron && !string.IsNullOrEmpty(definition.Trigger.CronExpression))
        {
            await _schedulingService.ScheduleOrUpdateAgentJob(id, definition.Trigger.CronExpression);
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var agent = await _db.Agents.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (agent == null) return NotFound();

        // Tenta parar antes de excluir
        try { await _containerManager.StopAgentAsync(agent.Id, default); } catch { }

        await _db.Agents.DeleteOneAsync(a => a.Id == id);

        return RedirectToAction(nameof(Index));
    }
}
