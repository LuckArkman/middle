// src/Services/IntelligentAutomationSaaS.Orchestrator/Services/QuotaService.cs

using IntelligentAutomation.Application.Enums;
using IntelligentAutomation.Application.Interfaces;
using IntelligentAutomation.Domain.Enums;
using IntelligentAutomation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IntelligentAutomation.Orchestrator.Services;

public class QuotaService : IQuotaService
{
    private readonly ApplicationDbContext _context;

    public QuotaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuotaCheckResult> CheckAgentCreationQuotaAsync(Guid userId)
    {
        // Encontra a assinatura ativa do usuário e inclui os detalhes do plano
        var activeSubscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

        if (activeSubscription?.Plan is null)
        {
            return QuotaCheckResult.NoActiveSubscription;
        }

        // Conta quantos agentes o usuário já possui
        var currentAgentCount = await _context.Agents.CountAsync(a => a.UserId == userId);
        
        if (currentAgentCount >= activeSubscription.Plan.MaxActiveAgents)
        {
            return QuotaCheckResult.MaxAgentsReached;
        }

        return QuotaCheckResult.Allowed;
    }
}