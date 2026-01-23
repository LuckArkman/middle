using IntelligentAutomation.Dtos;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Domain.Workflow;

namespace IntelligentAutomation.BlazorApp.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    // --- Métodos de Admin para Planos ---
    public async Task<List<Plan>> GetPlansAsync() =>
        await _httpClient.GetFromJsonAsync<List<Plan>>("/admin/plans") ?? new List<Plan>();

    public Task CreatePlanAsync(Plan plan) =>
        _httpClient.PostAsJsonAsync("/admin/plans", plan);

    public Task UpdatePlanAsync(Plan plan) =>
        _httpClient.PutAsJsonAsync($"/admin/plans/{plan.Id}", plan);

    public Task DeletePlanAsync(string id) =>
        _httpClient.DeleteAsync($"/admin/plans/{id}");

    // --- Métodos de Admin para Módulos ---
    public async Task<List<ModuleManifest>> GetModuleManifestsAsync() =>
        await _httpClient.GetFromJsonAsync<List<ModuleManifest>>("/admin/modules") ?? new List<ModuleManifest>();

    public Task CreateModuleManifestAsync(ModuleManifest manifest) =>
        _httpClient.PostAsJsonAsync("/admin/modules", manifest);

    public async Task<List<AgentDto>> GetAgentsAsync() =>
        await _httpClient.GetFromJsonAsync<List<AgentDto>>("/orchestrator/agents") ?? new List<AgentDto>();

    public async Task<AgentDto> CreateAgentAsync(CreateAgentDto agent)
    {
        var response = await _httpClient.PostAsJsonAsync<CreateAgentDto>("/orchestrator/agents", agent);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AgentDto>() ?? throw new Exception("Failed to deserialize agent.");
    }

    public Task StopAgentAsync(string agentId) =>
        _httpClient.PostAsync($"/orchestrator/agents/{agentId}/stop", null);
    public async Task<Dictionary<string, List<ModuleManifest>>> GetModuleCatalog() =>
        await _httpClient.GetFromJsonAsync<Dictionary<string, List<ModuleManifest>>>("/modules/catalog") ?? new Dictionary<string, List<ModuleManifest>>();

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

    public async Task RegisterAsync(RegisterRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("/auth/register", request);
        if (!response.IsSuccessStatusCode)
        {
            // Tenta ler a mensagem de erro da API e a lança como uma exceção
            var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(errorContent?.Message ?? "Ocorreu um erro desconhecido durante o registro.");
        }
    }

    // Classe auxiliar para desserializar a resposta de erro
    private class ErrorResponse
    {
        public string? Message { get; set; }
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("/auth/login", request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(errorContent?.Message ?? "Erro desconhecido durante o login.");
        }
        return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
    }
}