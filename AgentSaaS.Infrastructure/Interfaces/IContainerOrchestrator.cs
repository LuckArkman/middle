public interface IContainerOrchestrator
{
    Task<string> StartAgentAsync(Guid agentId, string image, Dictionary<string, string> envVars);
    Task StopAgentAsync(string containerId);
    Task<Stream> GetLogsStreamAsync(string containerId);
}