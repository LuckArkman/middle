using System.Collections.Concurrent;
using System.Reflection;
using IntelligentAutomation.AgentRuntime.Interfaces;
using IntelligentAutomation.Domain.Workflow;

namespace IntelligentAutomation.AgentRuntime.Modules;

public class ModuleRunner : IModuleRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _moduleRegistry = new();

    public ModuleRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        RegisterModules();
    }

    private void RegisterModules()
    {
        var moduleTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IModule)));

        foreach (var type in moduleTypes)
        {
            var attribute = type.GetCustomAttribute<ModuleAttribute>();
            var typeName = attribute?.ModuleType ?? type.Name.Replace("Module", string.Empty);

            _moduleRegistry[typeName] = type;
        }
    }

    public async Task<object> RunAsync(ModuleNode moduleNode, ConcurrentDictionary<string, object> context)
    {
        if (!_moduleRegistry.TryGetValue(moduleNode.ModuleType, out var moduleType))
        {
            throw new NotSupportedException($"Módulo do tipo '{moduleNode.ModuleType}' não é suportado.");
        }

        // Usa o provedor de serviços para criar uma instância do módulo, injetando suas dependências
        var moduleInstance = (IModule)_serviceProvider.GetRequiredService(moduleType);
        return await moduleInstance.ExecuteAsync(moduleNode.Parameters, context);
    }
}