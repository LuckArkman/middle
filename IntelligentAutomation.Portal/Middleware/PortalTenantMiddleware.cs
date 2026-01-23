using IntelligentAutomation.Interfaces;
using System.Security.Claims;

namespace IntelligentAutomation.Portal.Middleware;

public class PortalTenantMiddleware
{
    private readonly RequestDelegate _next;

    public PortalTenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;
            if (!string.IsNullOrEmpty(tenantIdClaim))
            {
                tenantService.SetTenantId(tenantIdClaim);
            }
        }
        else
        {
            // Fallback para subdomínio antes do login se necessário
            var host = context.Request.Host.Host;
            var parts = host.Split('.');
            if (parts.Length > 2)
            {
                tenantService.SetTenantId(parts[0]);
            }
        }

        await _next(context);
    }
}
