namespace IntelligentAutomation.Application.Interfaces;

public interface IAgentSchedulingService
{
    Task ScheduleOrUpdateAgentJob(Guid agentId, string cronExpression);
    Task UnscheduleAgentJob(Guid agentId);
}