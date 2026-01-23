using IntelligentAutomation.Domain.Entities;

namespace IntelligentAutomation.Interfaces;

public interface IAgentLogService
{
    Task LogAsync(string agentId, string message, string level = "Information", string category = "Execution", Dictionary<string, object>? details = null);
    Task<List<AgentLog>> GetLogsAsync(string agentId, int limit = 100);
}
