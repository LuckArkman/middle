using System.Text.Json.Serialization;

namespace IntelligentAutomation.Domain.Entities;


public class NotificationData
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}