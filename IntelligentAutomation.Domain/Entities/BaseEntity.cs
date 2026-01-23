using MongoDB.Bson.Serialization.Attributes;

namespace IntelligentAutomation.Domain.Entities;

public abstract class BaseEntity
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("tenantId")]
    public string TenantId { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    [BsonIgnoreIfNull]
    public DateTime? UpdatedAt { get; set; }
}