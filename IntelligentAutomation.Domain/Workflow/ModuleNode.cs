namespace IntelligentAutomation.Domain.Workflow;

// Dentro de src/Core/IntelligentAutomationSaaS.Domain/Workflow/BaseNode.cs
public class ModuleNode : BaseNode
{
    public string ModuleType { get; set; } = string.Empty;
    
    public BaseModuleParameters? Parameters { get; set; }
}