namespace IntelligentAutomation.Interfaces;

public interface ITenantService
{
    string GetTenantId();
    void SetTenantId(string tenantId);
}
