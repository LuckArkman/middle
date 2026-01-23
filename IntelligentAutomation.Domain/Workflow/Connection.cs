namespace IntelligentAutomation.Domain.Workflow;

public class Connection
{
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public string? Label { get; set; } // Ex: "True", "False"
}