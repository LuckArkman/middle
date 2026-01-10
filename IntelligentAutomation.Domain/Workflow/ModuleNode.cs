namespace IntelligentAutomation.Domain.Workflow;

public class ModuleNode : BaseNode
{
    public string ModuleType { get; set; } = string.Empty; // Ex: "HttpRequest", "SendEmail"
    public Dictionary<string, object> Parameters { get; set; } = new();
}