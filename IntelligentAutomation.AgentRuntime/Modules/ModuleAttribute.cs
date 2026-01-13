namespace IntelligentAutomation.AgentRuntime.Modules;

/// <summary>
/// Atributo para registrar um módulo com um nome de tipo único.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ModuleAttribute : Attribute
{
    public string ModuleType { get; }

    public ModuleAttribute(string moduleType)
    {
        ModuleType = moduleType;
    }
}