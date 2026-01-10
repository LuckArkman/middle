namespace AgentSaaS.Core.Entities;

public class AgentVersion
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public int VersionNumber { get; set; }
    public string SystemPrompt { get; set; }
    public string LlmModel { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
}