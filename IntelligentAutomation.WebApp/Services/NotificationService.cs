using Microsoft.AspNetCore.SignalR.Client;

namespace IntelligentAutomation.WebApp.Services;

public class NotificationService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    public event Action<string, string>? OnAgentStatusChanged;

    public async Task StartConnectionAsync()
    {
        // Assumindo que o Orquestrador expõe um hub em /notificationhub
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5076/notificationhub") // URL do Orquestrador
            .Build();

        _hubConnection.On<string, string>("AgentStatusChanged", (agentId, newStatus) =>
        {
            OnAgentStatusChanged?.Invoke(agentId, newStatus);
        });

        await _hubConnection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}