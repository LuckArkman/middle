using System.Collections.Concurrent;
using IntelligentAutomation.AgentRuntime.Interfaces;
using IntelligentAutomation.Domain.Workflow;

namespace IntelligentAutomation.AgentRuntime.Modules;

[Module("HttpRequest")]
public class HttpRequestModule : IModule
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpRequestModule> _logger;

    public HttpRequestModule(IHttpClientFactory httpClientFactory, ILogger<HttpRequestModule> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<object> ExecuteAsync(BaseModuleParameters? parameters, ConcurrentDictionary<string, object> context)
    {
        if (parameters is not HttpRequestModuleParameters httpParams)
        {
            throw new ArgumentException("Parâmetros inválidos para o módulo HttpRequest.");
        }
        
        _logger.LogInformation("Executando requisição HTTP para {Url}", httpParams.Url);
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(httpParams.Url); // Lógica simplificada
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return content; // O resultado é o corpo da resposta
    }
}