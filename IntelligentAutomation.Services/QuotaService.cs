using IntelligentAutomation.Enums;
using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Infrastructure.Persistence;
using MongoDB.Driver;

namespace IntelligentAutomation.Services;

public class QuotaService : IQuotaService
{
    private readonly MongoDbContext _db;

    public QuotaService(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<QuotaCheckResult> CheckAgentCreationQuotaAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return QuotaCheckResult.NoActiveSubscription;
        }

        var user = await _db.Users.Find(u => u.Id == userGuid).FirstOrDefaultAsync();
        if (user == null || string.IsNullOrEmpty(user.CurrentSubscriptionId))
        {
            return QuotaCheckResult.NoActiveSubscription;
        }

        if (!Guid.TryParse(user.CurrentSubscriptionId, out var subId))
        {
            return QuotaCheckResult.NoActiveSubscription;
        }

        var activeSubscription = await _db.Subscriptions.Find(s => s.Id == subId).FirstOrDefaultAsync();

        if (activeSubscription == null || activeSubscription.Status != Domain.Enums.SubscriptionStatus.Active)
        {
            return QuotaCheckResult.NoActiveSubscription;
        }

        var plan = await _db.Plans.Find(p => p.Id == activeSubscription.PlanId).FirstOrDefaultAsync();
        if (plan == null)
        {
            return QuotaCheckResult.NoActiveSubscription;
        }

        var currentAgentCount = await _db.Agents.CountDocumentsAsync(a => a.UserId == userId);

        if (currentAgentCount >= plan.MaxAgents)
        {
            return QuotaCheckResult.MaxAgentsReached;
        }

        return QuotaCheckResult.Allowed;
    }
}