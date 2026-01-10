namespace AgentSaaS.Core.Entities;

public class AgentMessage
{
    public long Id { get; set; }
    public Guid AgentId { get; set; } // FK
    public string Role { get; set; } // "User", "Assistant", "System", "Tool"
    public string Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    // Metadados importantes para debugging
    public int TokenCount { get; set; }
    public string ContentType { get; set; } = "text"; // "text" ou "image_url"
    public string? MediaUrl { get; set; } // URL da imagem no Azure Blob Storage / S3
}