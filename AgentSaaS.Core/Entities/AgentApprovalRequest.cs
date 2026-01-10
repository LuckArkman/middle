namespace AgentSaaS.Core.Entities;

public class AgentApprovalRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AgentId { get; set; }
    public string FunctionName { get; set; }
    public string ArgumentsJson { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}