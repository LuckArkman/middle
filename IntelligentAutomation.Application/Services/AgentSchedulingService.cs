using IntelligentAutomation.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace IntelligentAutomation.Orchestrator.Services;

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
            await UnscheduleAgentJob(agentId); // Remove o antigo para garantir que o gatilho seja atualizado
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
    public Task Execute(IJobExecutionContext context)
    {
        throw new NotImplementedException();
    }
}