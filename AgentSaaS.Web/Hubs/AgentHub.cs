using Microsoft.AspNetCore.SignalR;

namespace AgentSaaS.Web.Hubs;

public class AgentHub : Hub
{
    // O front-end entrará no grupo específico do Agente para ouvir logs
    public async Task JoinAgentGroup(string agentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, agentId);
    }
}