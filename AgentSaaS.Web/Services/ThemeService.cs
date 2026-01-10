using AgentSaaS.Infrastructure.Data;
using AgentSaaS.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AgentSaaS.Web.Services;

public class ThemeService
{
    private readonly ITenantProvider _tenantProvider;
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache; // Cache é CRUCIAL aqui para não bater no banco a cada F5

    public async Task<TenantThemeViewModel> GetCurrentThemeAsync()
    {
        var tenantId = _tenantProvider.GetTenantId();
        
        return await _cache.GetOrCreateAsync($"theme_{tenantId}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            
            var tenant = await _context.Tenants.FindAsync(tenantId);
            return new TenantThemeViewModel 
            {
                PrimaryColor = tenant.BrandColor,
                LogoUrl = tenant.LogoUrl ?? "/images/default-logo.png",
                CustomCss = tenant.CssOverride
            };
        });
    }
}