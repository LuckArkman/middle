using IntelligentAutomation.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace IntelligentAutomation.Services;

public class AgentSchedulingService : IAgentSchedulingService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<AgentSchedulingService> _logger;

    public AgentSchedulingService(ISchedulerFactory schedulerFactory, ILogger<AgentSchedulingService> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task ScheduleOrUpdateAgentJob(Guid agentId, string cronExpression)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var jobKey = new JobKey(agentId.ToString());

        if (await scheduler.CheckExists(jobKey))
        {
            _logger.LogInformation("Job para o Agente {AgentId} já existe. Reagendando...", agentId);
            await UnscheduleAgentJob(agentId);
        }

        _logger.LogInformation("Agendando novo job para o Agente {AgentId} com a expressão cron: '{Cron}'", agentId, cronExpression);

        IJobDetail job = JobBuilder.Create<AgentExecutionJob>()
            .WithIdentity(jobKey)
            .UsingJobData("AgentId", agentId.ToString())
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(agentId.ToString() + "-trigger")
            .WithCronSchedule(cronExpression)
            .ForJob(jobKey)
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }

    public async Task UnscheduleAgentJob(Guid agentId)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var jobKey = new JobKey(agentId.ToString());

        if (await scheduler.CheckExists(jobKey))
        {
            _logger.LogInformation("Removendo agendamento para o Agente {AgentId}", agentId);
            await scheduler.DeleteJob(jobKey);
        }
    }
}

public class AgentExecutionJob : IJob
{
    private readonly IContainerManagerService _containerManager;
    private readonly ILogger<AgentExecutionJob> _logger;
    private readonly IAgentLogService _logService;

    public AgentExecutionJob(IContainerManagerService containerManager, ILogger<AgentExecutionJob> logger, IAgentLogService logService)
    {
        _containerManager = containerManager;
        _logger = logger;
        _logService = logService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var agentIdStr = context.MergedJobDataMap.GetString("AgentId");
        if (string.IsNullOrEmpty(agentIdStr)) return;

        var agentId = Guid.Parse(agentIdStr);
        _logger.LogInformation("Executando Job Agendado para Agente {AgentId}", agentId);

        try
        {
            // O provisionamento e início agora são via Container Manager
            await _containerManager.StartAgentAsync(agentId, context.CancellationToken);
            await _logService.LogAsync(agentIdStr, "Execução agendada iniciada via Quartz.", "Information", "Scheduling");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao executar job agendado para o agente {AgentId}", agentId);
            await _logService.LogAsync(agentIdStr, $"Falha no agendamento: {ex.Message}", "Error", "Scheduling");
        }
    }
}