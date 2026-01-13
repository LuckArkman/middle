using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace IntelligentAutomation.WebApp.Services;

public class NotificationService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private HubConnection? _hubConnection;
    public event Action<string, string>? OnAgentStatusChanged;

    // Injeta o IJSRuntime para acessar o token no sessionStorage
    public NotificationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task StartConnectionAsync()
    {
        if (_hubConnection is not null)
        {
            return; // Já conectado ou conectando
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5076/notificationhub", options =>
            {
                // ---- INÍCIO DA CORREÇÃO DE AUTENTICAÇÃO ----
                // Esta função é chamada antes de cada requisição do SignalR (incluindo a de conexão),
                // permitindo-nos obter o token mais recente e anexá-lo.
                options.AccessTokenProvider = async () =>
                    await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "authToken");
                // ---- FIM DA CORREÇÃO DE AUTENTICAÇÃO ----
            })
            .WithAutomaticReconnect() // ---- INÍCIO DA CORREÇÃO DE RESILIÊNCIA ----
            .Build();

        _hubConnection.On<string, string>("AgentStatusChanged", (agentId, newStatus) =>
        {
            OnAgentStatusChanged?.Invoke(agentId, newStatus);
        });

        try
        {
            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar ao SignalR Hub: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}