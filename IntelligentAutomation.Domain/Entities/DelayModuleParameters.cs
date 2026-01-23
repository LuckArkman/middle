using IntelligentAutomation.Domain.Workflow;

namespace IntelligentAutomation.Domain.Entities;

public class DelayModuleParameters : BaseModuleParameters
{
    public int DelayMilliseconds { get; set; } = 1000;
}
