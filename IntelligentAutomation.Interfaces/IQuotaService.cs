using IntelligentAutomation.Enums;
using IntelligentAutomation.Enums;

namespace IntelligentAutomation.Interfaces;

public interface IQuotaService
{
    Task<QuotaCheckResult> CheckAgentCreationQuotaAsync(string userId);
}