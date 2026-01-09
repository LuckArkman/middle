using AgentSaaS.Core.Enums;
using AgentSaaS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgentSaaS.Web.Controllers;

[Authorize]
public class AgentsController : Controller
{
    private readonly AppDbContext _context;

    public AgentsController(AppDbContext context)
    {
        _context = context;
    }

    // Listagem (Dashboard)
    public async Task<IActionResult> Index()
    {
        var agents = await _context.Agents.ToListAsync();
        return View(agents);
    }

    // Ação de Controle (Fase 6.1)
    [HttpPost]
    public async Task<IActionResult> ToggleStatus(Guid id, string command)
    {
        var agent = await _context.Agents.FindAsync(id);
        if (agent == null) return NotFound();

        // O usuário apenas muda o status no banco.
        // O Orchestrator (Worker) fará o trabalho pesado de Docker.
        if (command == "START") agent.Status = AgentStatus.Executando;
        if (command == "STOP") agent.Status = AgentStatus.Parado;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Visualização de Logs (Fase 7)
    public async Task<IActionResult> Logs(Guid id)
    {
        var logs = await _context.AgentLogs
            .Where(l => l.AgentId == id)
            .OrderByDescending(l => l.Timestamp)
            .Take(100)
            .ToListAsync();
            
        return View(logs);
    }
}