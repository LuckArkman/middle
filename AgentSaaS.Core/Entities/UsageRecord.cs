namespace AgentSaaS.Core.Entities;

public class UsageRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid AgentId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int InputTokens { get; set; }  // O que você enviou
    public int OutputTokens { get; set; } // O que a IA respondeu
    public decimal EstimatedCost { get; set; } // Valor em Dólar calculado na hora
}