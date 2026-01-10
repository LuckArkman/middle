using System.Net.Http.Json;
using IntelligentAutomation.Application.Dtos; // DTO que já criamos
using IntelligentAutomation.Domain.Workflow; // Modelo do workflow que já criamos

namespace IntelligentAutomation.WebApp.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Futuramente: public Task<AgentDto[]> GetAgents() => ...
    
    public async Task UpdateAgentDefinition(Guid agentId, WorkflowDefinition definition)
    {
        var response = await _httpClient.PutAsJsonAsync($"/orchestrator/agents/{agentId}/definition", definition);
        response.EnsureSuccessStatusCode();
    }
}