using AgentSaaS.Core.Entities;
using AgentSaaS.Infrastructure.Data;

namespace AgentSaaS.Infrastructure.Services;

public class UsageService
{
    private readonly AppDbContext _context;
    
    // Preços do GPT-4o (Exemplo)
    private const decimal PricePer1KInput = 0.005m;
    private const decimal PricePer1KOutput = 0.015m;

    public UsageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task TrackUsageAsync(Guid tenantId, Guid agentId, int input, int output)
    {
        var cost = (input / 1000m * PricePer1KInput) + (output / 1000m * PricePer1KOutput);

        var record = new UsageRecord
        {
            TenantId = tenantId,
            AgentId = agentId,
            InputTokens = input,
            OutputTokens = output,
            EstimatedCost = cost
        };

        _context.UsageRecords.Add(record);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasQuotaAsync(Guid tenantId)
    {
        // Regra: Limite de $10.00 por mês para plano Básico
        var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        
        var totalCost = await _context.UsageRecords
            .Where(u => u.TenantId == tenantId && u.Timestamp >= currentMonthStart)
            .SumAsync(u => u.EstimatedCost);

        // Idealmente, esse limite vem da tabela 'Tenants' ou 'Plans'
        return totalCost < 10.00m; 
    }
}