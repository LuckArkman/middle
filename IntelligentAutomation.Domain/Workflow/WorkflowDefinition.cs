namespace IntelligentAutomation.Domain.Workflow;

public class WorkflowDefinition
{
    public TriggerNode Trigger { get; set; } = new();
    public List<BaseNode> Nodes { get; set; } = new();
    public List<Connection> Connections { get; set; } = new();
}