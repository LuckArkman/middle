namespace IntelligentAutomation.ContainerManager.Interfaces;

public interface IDockerService
{
    Task<string> CreateAgentContainerAsync(Guid agentId, string imageTag);
    Task StartContainerAsync(string containerId);
    Task StopContainerAsync(string containerId);
    Task RemoveContainerAsync(string containerId);
}
