namespace AgentSaaS.Core.Entities;

public class AgentInboxMessage
{
    public string FromNumber { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
}