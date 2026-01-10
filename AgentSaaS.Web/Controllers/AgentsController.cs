using AgentSaaS.Core.Enums;
using AgentSaaS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class AgentsController : Controller
{
    private readonly AppDbContext _context;

    public AgentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Start(Guid id)
    {
        // O QueryFilter garante que eu só ache o agente se ele for do meu Tenant
        var agent = await _context.Agents.FindAsync(id);
        
        if (agent == null) return NotFound();

        // Apenas mudamos o status. 
        // O ORCHESTRATOR (Worker) vai ver essa mudança e subir o Docker.
        agent.Status = AgentStatus.Executando;
        
        await _context.SaveChangesAsync();
        
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Stop(Guid id)
    {
        var agent = await _context.Agents.FindAsync(id);
        if (agent == null) return NotFound();

        agent.Status = AgentStatus.Parado;
        // O ORCHESTRATOR vai ver isso e matar o container
        
        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }
}