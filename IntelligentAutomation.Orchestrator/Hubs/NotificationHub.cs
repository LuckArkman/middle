using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IntelligentAutomation.Orchestrator.Hubs;

[Authorize] // CRUCIAL: Apenas usuários autenticados podem se conectar a este hub.
public class NotificationHub : Hub
{
    // Este hub será usado para enviar mensagens do servidor para os clientes.
    // O cliente escutará por eventos como "AgentStatusChanged".

    // Exemplo de como usar este hub a partir de outro serviço:
    //
    // private readonly IHubContext<NotificationHub> _hubContext;
    //
    // public async Task UpdateAgentStatus(string userId, string agentId, string newStatus)
    // {
    //     // Envia a mensagem apenas para o usuário específico.
    //     await _hubContext.Clients.User(userId).SendAsync("AgentStatusChanged", agentId, newStatus);
    // }

    public override async Task OnConnectedAsync()
    {
        // Opcional: Adicionar lógica quando um usuário se conecta.
        // O 'Context.UserIdentifier' conterá o ID do usuário vindo do token JWT.
        await base.OnConnectedAsync();
    }
}