// src/Core/IntelligentAutomationSaaS.Application/Services/ContainerManagerService.cs
using System.Net.Http.Json;
using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Dtos;
using Microsoft.Extensions.Logging;

namespace IntelligentAutomation.Services;

public class ContainerManagerService : IContainerManagerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ContainerManagerService> _logger;

    public ContainerManagerService(HttpClient httpClient, ILogger<ContainerManagerService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task ProvisionAgentAsync(ProvisionAgentRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enviando requisição de provisionamento para o Container Manager. Agente ID: {AgentId}", request.AgentId);
        var response = await _httpClient.PostAsJsonAsync("/containers/provision", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task StartAgentAsync(Guid agentId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enviando requisição de início para o Container Manager. Agente ID: {AgentId}", agentId);
        var response = await _httpClient.PostAsync($"/containers/{agentId}/start", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task StopAgentAsync(Guid agentId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enviando requisição de parada para o Container Manager. Agente ID: {AgentId}", agentId);
        var response = await _httpClient.PostAsync($"/containers/{agentId}/stop", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}