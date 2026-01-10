namespace AgentSaaS.Core.Entities;

public class AgentSchedule
{
    public int Id { get; set; }
    public Guid AgentId { get; set; }
    public string CronExpression { get; set; } // Ex: "0 8 * * *" (Todo dia as 8h)
    public string GoalPrompt { get; set; }     // "Verifique pendências no CRM e avise o gerente."
    public bool IsActive { get; set; } = true;
    public DateTime? LastRun { get; set; }
}