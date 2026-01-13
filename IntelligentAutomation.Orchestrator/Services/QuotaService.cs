using IntelligentAutomation.Application.Enums;
using IntelligentAutomation.Application.Interfaces;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Infrastructure.Persistence;
using MongoDB.Driver;

namespace IntelligentAutomation.Orchestrator.Services;

public class QuotaService : IQuotaService
{
    private readonly MongoDbContext _context;

    public QuotaService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<QuotaCheckResult> CheckAgentCreationQuotaAsync(string userId)
    {
        // Encontra o usuário para obter a ID da assinatura ativa
        var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user?.CurrentSubscriptionId == null)
        {
            return QuotaCheckResult.NoActiveSubscription;
        }

        // Encontra a assinatura ativa e, em seguida, o plano correspondente
        var activeSubscription = await _context.Subscriptions.Find(s => s.Id == user.CurrentSubscriptionId).FirstOrDefaultAsync();
        if (activeSubscription?.Status != Domain.Enums.SubscriptionStatus.Active)
        {
            return QuotaCheckResult.NoActiveSubscription;
        }
        
        var plan = await _context.Plans.Find(p => p.Id == activeSubscription.PlanId).FirstOrDefaultAsync();
        if (plan == null)
        {
            // Caso de inconsistência de dados, tratar como falta de assinatura
            return QuotaCheckResult.NoActiveSubscription;
        }

        // Conta quantos agentes o usuário já possui
        var currentAgentCount = await _context.Agents.CountDocumentsAsync(a => a.UserId == userId);
        
        if (currentAgentCount >= plan.MaxActiveAgents)
        {
            return QuotaCheckResult.MaxAgentsReached;
        }

        return QuotaCheckResult.Allowed;
    }
}