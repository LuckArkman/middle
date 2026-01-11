using IntelligentAutomation.Domain.Enums;

namespace IntelligentAutomation.Domain.Entities;

public class Subscription : BaseEntity
{
    // CORREÇÃO: Alterado de Guid para string para corresponder ao ID do User
    public string UserId { get; set; } = string.Empty;

    // CORREÇÃO: Alterado de Guid para string para corresponder ao ID do Plan
    public string PlanId { get; set; } = string.Empty;
    
    public SubscriptionStatus Status { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public string? StripeSubscriptionId { get; set; } // Renomear para PaymentGatewaySubscriptionId seria melhor, mas mantido por consistência
}