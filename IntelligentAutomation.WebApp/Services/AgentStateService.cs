using IntelligentAutomation.Application.Dtos;
using IntelligentAutomation.Domain.Entities;

namespace IntelligentAutomation.WebApp.Services;

public class AgentStateService
{
    private readonly ApiClient _apiClient;

    public AgentStateService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<AgentDto>? Agents { get; private set; }
    public event Action? OnChange;

    public async Task LoadAgentsAsync()
    {
        Agents = await _apiClient.GetAgentsAsync();
        NotifyStateChanged();
    }
    
    public async Task CreateAgentAsync(CreateAgentDto agent)
    {
        var newAgent = await _apiClient.CreateAgentAsync(agent);
        Agents?.Add(newAgent); // Adiciona à lista local
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}