using AgentSaaS.Core.Enums;
using AgentSaaS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgentSaaS.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "SuperAdmin")]
[Route("admin/[controller]")]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    // Visão Geral do Ecossistema
    public async Task<IActionResult> Index()
    {
        var stats = new AdminStatsViewModel
        {
            TotalTenants = await _context.Tenants.IgnoreQueryFilters().CountAsync(),
            TotalAgentsRunning = await _context.Agents.IgnoreQueryFilters().CountAsync(a => a.Status == AgentStatus.Executando),
            TotalRevenue = await _context.Tenants.IgnoreQueryFilters()
                .Where(t => t.PlanType != "Basic")
                .CountAsync() * 99.00m // Exemplo simplificado de cálculo
        };

        return View(stats);
    }

    // Botão de Pânico: Parar Forçadamente um Agente travado
    [HttpPost("force-stop/{agentId}")]
    public async Task<IActionResult> ForceStop(Guid agentId)
    {
        var agent = await _context.Agents.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == agentId);
        if (agent != null)
        {
            agent.Status = AgentStatus.Parado;
            // O Orchestrator pegará essa mudança e matará o container
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}