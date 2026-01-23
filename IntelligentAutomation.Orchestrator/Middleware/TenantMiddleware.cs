using IntelligentAutomation.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace IntelligentAutomation.Orchestrator.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        // 1. Prioridade: JWT Claim (Mais seguro para usuários logados)
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;
            if (!string.IsNullOrEmpty(tenantIdClaim))
            {
                tenantService.SetTenantId(tenantIdClaim);
                await _next(context);
                return;
            }
        }

        // 2. Fallback: Header (Útil para APIs de terceiros ou integrações)
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
        {
            tenantService.SetTenantId(tenantIdHeader.ToString());
        }
        // 3. Fallback: Subdomínio
        else if (context.Request.Host.HasValue)
        {
            var parts = context.Request.Host.Host.Split('.');
            if (parts.Length > 2)
            {
                tenantService.SetTenantId(parts[0]);
            }
        }

        await _next(context);
    }
}
