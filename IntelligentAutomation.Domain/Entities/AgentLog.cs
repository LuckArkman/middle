using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IntelligentAutomation.Domain.Entities;

public class AgentLog : BaseEntity
{
    [BsonElement("agentId")]
    public string AgentId { get; set; } = string.Empty;

    [BsonElement("level")]
    public string Level { get; set; } = "Information"; // Information, Warning, Error, Critical

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = "Execution"; // Execution, Error, Integration, Lifecycle

    [BsonElement("details")]
    public Dictionary<string, object>? Details { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
