using AgentSaaS.Infrastructure.Data;
using AgentSaaS.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Driver.Linq;

namespace AgentSaaS.Web.Controllers;

public class CheckAgentLimitAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Recupera serviços do DI container
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var tenantProvider = context.HttpContext.RequestServices.GetRequiredService<ITenantProvider>();
        
        var tenantId = tenantProvider.GetTenantId();
        
        // Dados do Tenant e contagem atual
        var tenant = await dbContext.Tenants.FindAsync(tenantId);
        var currentAgents = await dbContext.Agents.CountAsync(a => a.TenantId == tenantId);

        if (currentAgents >= tenant.MaxAgents)
        {
            // Bloqueia a ação e retorna erro ou redireciona para upgrade
            var controller = (Controller)context.Controller;
            controller.TempData["Error"] = $"Seu plano {tenant.PlanType} permite apenas {tenant.MaxAgents} agentes. Faça o Upgrade!";
            
            context.Result = new RedirectToActionResult("Index", "Agents", null);
            return;
        }

        await next();
    }
}