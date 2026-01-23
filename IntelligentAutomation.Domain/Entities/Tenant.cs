namespace IntelligentAutomation.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty; // Subdomínio ou identificador único
    public bool IsActive { get; set; } = true;
    public string? SubscriptionPlanId { get; set; }
}
