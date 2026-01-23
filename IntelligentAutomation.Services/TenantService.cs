using IntelligentAutomation.Interfaces;

namespace IntelligentAutomation.Services;

public class TenantService : ITenantService
{
    private string _tenantId = string.Empty;

    public string GetTenantId()
    {
        return _tenantId;
    }

    public void SetTenantId(string tenantId)
    {
        _tenantId = tenantId;
    }
}
