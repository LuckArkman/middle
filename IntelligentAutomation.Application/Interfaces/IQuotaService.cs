using IntelligentAutomation.Application.Enums;

namespace IntelligentAutomation.Application.Interfaces;

public interface IQuotaService
{
    Task<QuotaCheckResult> CheckAgentCreationQuotaAsync(Guid userId);
}