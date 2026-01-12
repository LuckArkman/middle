using System.Net.Http.Json;
using IntelligentAutomation.Application.Dtos;
using IntelligentAutomation.Domain.Entities; // DTO que já criamos
using IntelligentAutomation.Domain.Workflow; // Modelo do workflow que já criamos

namespace IntelligentAutomation.WebApp.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    
    // --- Métodos de Admin para Planos ---
    public Task<List<Plan>> GetPlansAsync() => 
        _httpClient.GetFromJsonAsync<List<Plan>>("/admin/plans");

    public Task CreatePlanAsync(Plan plan) => 
        _httpClient.PostAsJsonAsync("/admin/plans", plan);

    public Task UpdatePlanAsync(Plan plan) => 
        _httpClient.PutAsJsonAsync($"/admin/plans/{plan.Id}", plan);

    public Task DeletePlanAsync(string id) => 
        _httpClient.DeleteAsync($"/admin/plans/{id}");

// --- Métodos de Admin para Módulos ---
    public Task<List<ModuleManifest>> GetModuleManifestsAsync() =>
        _httpClient.GetFromJsonAsync<List<ModuleManifest>>("/admin/modules");

    public Task CreateModuleManifestAsync(ModuleManifest manifest) =>
        _httpClient.PostAsJsonAsync("/admin/modules", manifest);
    
    public Task<List<AgentDto>> GetAgentsAsync() =>
        _httpClient.GetFromJsonAsync<List<AgentDto>>("/orchestrator/agents");

    public Task<AgentDto> CreateAgentAsync(CreateAgentDto agent) =>
        _httpClient.PostAsJsonAsync<CreateAgentDto>("/orchestrator/agents", agent)
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<AgentDto>()).Unwrap();

    public Task StopAgentAsync(string agentId) =>
        _httpClient.PostAsync($"/orchestrator/agents/{agentId}/stop", null);
    public Task<Dictionary<string, List<ModuleManifest>>> GetModuleCatalog() =>
        _httpClient.GetFromJsonAsync<Dictionary<string, List<ModuleManifest>>>("/modules/catalog");

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Futuramente: public Task<AgentDto[]> GetAgents() => ...
    
    public async Task UpdateAgentDefinition(string agentId, WorkflowDefinition definition)
    {
        var response = await _httpClient.PutAsJsonAsync($"/orchestrator/agents/{agentId}/definition", definition);
        response.EnsureSuccessStatusCode();
    }
}