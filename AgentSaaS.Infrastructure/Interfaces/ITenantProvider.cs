namespace AgentSaaS.Infrastructure.Interfaces;

public interface ITenantProvider
{
    Guid GetTenantId();
}