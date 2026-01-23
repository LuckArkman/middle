using System.Collections.Concurrent;
using IntelligentAutomation.AgentRuntime.Interfaces;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Domain.Workflow;
using Microsoft.Extensions.Logging;

namespace IntelligentAutomation.AgentRuntime.Modules;

[Module("delay")]
public class DelayModule : IModule
{
    private readonly ILogger<DelayModule> _logger;

    public DelayModule(ILogger<DelayModule> logger)
    {
        _logger = logger;
    }

    public async Task<object> ExecuteAsync(BaseModuleParameters? parameters, ConcurrentDictionary<string, object> context)
    {
        if (parameters is not DelayModuleParameters delayParams)
        {
            throw new ArgumentException("Parâmetros inválidos para o módulo Delay.");
        }

        _logger.LogInformation("Aguardando {Ms}ms...", delayParams.DelayMilliseconds);
        await Task.Delay(delayParams.DelayMilliseconds);

        return true;
    }
}
