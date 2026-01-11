using IntelligentAutomation.Application.Dtos;

namespace IntelligentAutomation.Application.Interfaces;

public interface IContainerManagerService
{
    Task ProvisionAgentAsync(ProvisionAgentRequest request, CancellationToken cancellationToken = default);
    Task StartAgentAsync(Guid agentId, CancellationToken cancellationToken = default);
    Task StopAgentAsync(Guid agentId, CancellationToken cancellationToken = default);
}