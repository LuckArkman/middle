using IntelligentAutomation.Domain.Enums;

namespace IntelligentAutomation.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public string? StripeSubscriptionId { get; set; } // ID da assinatura no gateway de pagamento

    public Plan? Plan { get; set; } // Propriedade de navegação
}