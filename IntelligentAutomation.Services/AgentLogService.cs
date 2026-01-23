using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Interfaces;
using MongoDB.Driver;

namespace IntelligentAutomation.Services;

public class AgentLogService : IAgentLogService
{
    private readonly IMongoCollection<AgentLog> _logs;
    private readonly ITenantService _tenantService;

    public AgentLogService(IMongoDatabase database, ITenantService tenantService)
    {
        _logs = database.GetCollection<AgentLog>("AgentLogs");
        _tenantService = tenantService;

        // Garante índices para performance
        var indexKeysDefinition = Builders<AgentLog>.IndexKeys.Ascending(l => l.AgentId).Ascending(l => l.TenantId).Descending(l => l.Timestamp);
        _logs.Indexes.CreateOne(new CreateIndexModel<AgentLog>(indexKeysDefinition));
    }

    public async Task LogAsync(string agentId, string message, string level = "Information", string category = "Execution", Dictionary<string, object>? details = null)
    {
        var log = new AgentLog
        {
            AgentId = agentId,
            TenantId = _tenantService.GetTenantId(),
            Message = message,
            Level = level,
            Category = category,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        await _logs.InsertOneAsync(log);
    }

    public async Task<List<AgentLog>> GetLogsAsync(string agentId, int limit = 100)
    {
        var tenantId = _tenantService.GetTenantId();
        return await _logs.Find(l => l.AgentId == agentId && l.TenantId == tenantId)
                          .SortByDescending(l => l.Timestamp)
                          .Limit(limit)
                          .ToListAsync();
    }
}
