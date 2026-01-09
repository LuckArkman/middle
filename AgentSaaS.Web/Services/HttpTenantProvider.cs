using AgentSaaS.Infrastructure.Interfaces;

namespace AgentSaaS.Web.Services;

public class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        // OPÇÃO 1: Pega do Claim do usuário logado (Recomendado para produção)
        var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId");
        if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantId))
        {
            return tenantId;
        }

        // OPÇÃO 2 (Para Testes/Dev): Pega do Header 'X-Tenant-ID'
        var header = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-ID"].FirstOrDefault();
        if (!string.IsNullOrEmpty(header) && Guid.TryParse(header, out var headerId))
        {
            return headerId;
        }

        // Fallback ou Erro
        throw new UnauthorizedAccessException("Tenant não identificado.");
    }
}