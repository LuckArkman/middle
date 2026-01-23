using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using IntelligentAutomation.AgentRuntime.Interfaces;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Domain.Workflow;

namespace IntelligentAutomation.AgentRuntime.Modules;

[Module("llm")]
public class LlmModule : IModule
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LlmModule(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<object> ExecuteAsync(BaseModuleParameters parameters, ConcurrentDictionary<string, object> context)
    {
        var p = parameters as LlmModuleParameters;
        if (p == null) throw new ArgumentException("Parâmetros inválidos para LlmModule.");

        // Substituição simples de templates: {{node_id}} pelo valor no context
        var userPrompt = ReplaceTemplates(p.UserPromptTemplate, context);

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey)) throw new Exception("OPENAI_API_KEY não configurada no ambiente.");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var requestBody = new
        {
            model = p.Model,
            messages = new[]
            {
                new { role = "system", content = p.SystemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = p.Temperature,
            max_tokens = p.MaxTokens
        };

        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions",
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);

        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? string.Empty;
    }

    private string ReplaceTemplates(string template, ConcurrentDictionary<string, object> context)
    {
        var result = template;
        foreach (var item in context)
        {
            result = result.Replace($"{{{{{item.Key}}}}}", item.Value?.ToString() ?? string.Empty);
        }
        return result;
    }
}
