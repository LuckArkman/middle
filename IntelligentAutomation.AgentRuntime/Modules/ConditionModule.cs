using System.Collections.Concurrent;
using IntelligentAutomation.AgentRuntime.Interfaces;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Domain.Workflow;

namespace IntelligentAutomation.AgentRuntime.Modules;

[Module("condition")]
public class ConditionModule : IModule
{
    public Task<object> ExecuteAsync(BaseModuleParameters parameters, ConcurrentDictionary<string, object> context)
    {
        var p = parameters as ConditionModuleParameters;
        if (p == null) throw new ArgumentException("Parâmetros inválidos para ConditionModule.");

        // Busca o valor da variável no contexto
        // A lógica de busca pode ser complexa (JSON path etc), por enquanto faremos simples
        object? sourceValue = null;
        if (context.TryGetValue(p.VariableName, out var val))
        {
            sourceValue = val;
        }
        else
        {
            // Tenta buscar de um nó específico se VariableName for um ID de nó
            sourceValue = context.FirstOrDefault(x => x.Key == p.VariableName).Value;
        }

        bool result = EvaluateCondition(sourceValue?.ToString() ?? string.Empty, p.Operator, p.ComparisonValue);

        return Task.FromResult<object>(result);
    }

    private bool EvaluateCondition(string source, string op, string target)
    {
        return op switch
        {
            "Equals" => source == target,
            "NotEquals" => source != target,
            "Contains" => source.Contains(target),
            "GreaterThan" => decimal.TryParse(source, out var s) && decimal.TryParse(target, out var t) && s > t,
            "LessThan" => decimal.TryParse(source, out var s2) && decimal.TryParse(target, out var t2) && s2 < t2,
            _ => false
        };
    }
}
