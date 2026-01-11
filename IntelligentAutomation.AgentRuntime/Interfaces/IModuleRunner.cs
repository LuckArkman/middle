using System.Collections.Concurrent;
using IntelligentAutomation.Domain.Workflow;

namespace IntelligentAutomation.AgentRuntime.Interfaces;

public interface IModuleRunner
{
    Task<object> RunAsync(ModuleNode moduleNode, ConcurrentDictionary<string, object> context);
}