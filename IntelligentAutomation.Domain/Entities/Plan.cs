namespace IntelligentAutomation.Domain.Entities;

public class Plan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int MaxActiveAgents { get; set; }
    public decimal MonthlyPrice { get; set; }
    public string? StripePriceId { get; set; } // ID do preço no gateway de pagamento
    public bool IsActive { get; set; } = true;
}