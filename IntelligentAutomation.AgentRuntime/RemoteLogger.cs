using Microsoft.Extensions.Logging;
using IntelligentAutomation.AgentRuntime;
using System.Net.Http.Json;

namespace IntelligentAutomation.AgentRuntime;

public class RemoteLogger : ILogger<WorkflowEngine>
{
    private readonly HttpClient _client;
    private readonly string _url;
    private readonly string _agentId;

    public RemoteLogger(HttpClient client, string url, string agentId)
    {
        _client = client;
        _url = url;
        _agentId = agentId;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        if (exception != null) message += " | " + exception.ToString();

        // Fogo e esquece (ou aguarda se necessário)
        _ = _client.PostAsJsonAsync($"{_url}/agents/{_agentId}/logs", new
        {
            Message = message,
            Level = logLevel.ToString(),
            Category = "Execution",
            Timestamp = DateTime.UtcNow
        });

        Console.WriteLine($"[{logLevel}] {message}");
    }
}
