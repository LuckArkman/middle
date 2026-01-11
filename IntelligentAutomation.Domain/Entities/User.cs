namespace IntelligentAutomation.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? StripeCustomerId { get; set; } // ID do cliente no gateway de pagamento
    public Guid? CurrentSubscriptionId { get; set; } // Referência à assinatura ativa
}