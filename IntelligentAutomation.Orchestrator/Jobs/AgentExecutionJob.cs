using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Services;
using Quartz;

namespace IntelligentAutomationSaaS.Orchestrator.Jobs;

[DisallowConcurrentExecution] 
public class AgentExecutionJob : IJob
{
    private readonly ILogger<AgentExecutionJob> _logger;
    private readonly IContainerManagerService _containerManagerService;

    public AgentExecutionJob(ILogger<AgentExecutionJob> logger, IContainerManagerService containerManagerService)
    {
        _logger = logger;
        _containerManagerService = containerManagerService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        // Extrai o ID do agente dos dados do job
        var agentId = context.JobDetail.JobDataMap.GetGuid("AgentId");

        if (agentId == Guid.Empty)
        {
            _logger.LogError("AgentExecutionJob foi acionado sem um AgentId válido.");
            return;
        }
        
        _logger.LogInformation("Gatilho acionado para o Agente ID: {AgentId}. Iniciando execução...", agentId);

        try
        {
            // Ação principal: instrui o Container Manager a iniciar o container do agente
            await _containerManagerService.StartAgentAsync(agentId, context.CancellationToken);
            _logger.LogInformation("Comando de início enviado com sucesso para o Agente ID: {AgentId}", agentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao executar o Agente ID: {AgentId}", agentId);
        }
    }
}