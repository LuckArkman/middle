using AgentSaaS.Core.Entities;
using AgentSaaS.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgentSaaS.Web.Controllers;

[Authorize]
public class MarketplaceController : Controller
{

    [HttpPost]
    public async Task<IActionResult> Install(int templateId)
    {
        var template = await _context.AgentTemplates.FindAsync(templateId);
        var tenantId = _tenantProvider.GetTenantId();

        // Cria um novo agente baseado no template
        var newAgent = new Agent
        {
            TenantId = tenantId,
            Name = $"{template.Name} (Cópia)",
            SystemPrompt = template.SystemPromptTemplate,
            Status = AgentStatus.Parado,
            // Copia configurações padrão...
        };

        _context.Agents.Add(newAgent);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Agents", new { id = newAgent.Id });
    }
}