namespace IntelligentAutomation.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? StripeCustomerId { get; set; } // Renomear para PaymentGatewayCustomerId seria melhor
    
    // CORREÇÃO: Alterado de Guid? para string? para corresponder ao ID da Subscription
    public string? CurrentSubscriptionId { get; set; } 
}