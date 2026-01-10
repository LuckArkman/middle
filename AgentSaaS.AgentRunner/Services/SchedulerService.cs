using AgentSaaS.Infrastructure.Data;
using Cronos;

public class SchedulerService
{
    private readonly AppDbContext _context;
    private readonly Guid _agentId;

    public SchedulerService(AppDbContext context, Guid agentId)
    {
        _context = context;
        _agentId = agentId;
    }

    public async Task CheckSchedulesAsync(Func<string, Task> onTriggerCallback)
    {
        var schedules = await _context.AgentSchedules
            .Where(s => s.AgentId == _agentId && s.IsActive)
            .ToListAsync();

        foreach (var job in schedules)
        {
            // Lógica simplificada: Verifica se deveria ter rodado no último minuto
            // Em produção, use Quartz.NET para precisão, mas isso funciona para MVP
            var expression = CronExpression.Parse(job.CronExpression);
            var nextRun = expression.GetNextOccurrence(job.LastRun ?? DateTime.UtcNow.AddDays(-1));

            if (nextRun.HasValue && nextRun.Value <= DateTime.UtcNow)
            {
                Console.WriteLine($"[CRON] Disparando tarefa: {job.GoalPrompt}");
                
                // Dispara a ação!
                await onTriggerCallback(job.GoalPrompt);

                job.LastRun = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}