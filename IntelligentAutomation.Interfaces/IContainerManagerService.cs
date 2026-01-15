using IntelligentAutomation.Dtos;

namespace IntelligentAutomation.Interfaces;

public interface IContainerManagerService
{
    Task ProvisionAgentAsync(ProvisionAgentRequest request, CancellationToken cancellationToken = default);
    Task StartAgentAsync(Guid agentId, CancellationToken cancellationToken = default);
    Task StopAgentAsync(Guid agentId, CancellationToken cancellationToken = default);
}