using System.Collections.Concurrent;
using IntelligentAutomation.Domain.Workflow;

namespace IntelligentAutomation.AgentRuntime.Interfaces;

public interface IModule
{
    Task<object> ExecuteAsync(BaseModuleParameters? parameters, ConcurrentDictionary<string, object> context);
}