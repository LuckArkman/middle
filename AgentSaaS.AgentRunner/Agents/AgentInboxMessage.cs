using AgentSaaS.AgentRunner.Plugins;

namespace AgentSaaS.AgentRunner.Agents;

// DTO de Mensagem enriquecido
public class AgentInboxMessage
{
    public string Channel { get; set; } // "WhatsApp" ou "Sandbox"
    public string ConnectionId { get; set; } // ID do SignalR (se Sandbox)
    public string FromNumber { get; set; }
    // ...
}

/* 
// Na lógica de resposta do Loop Principal
if (msg.Channel == "WhatsApp")
{
    await WhatsAppPlugin.SendMessage(msg.FromNumber, result.Content);
}
else if (msg.Channel == "Sandbox")
{
    // Envia de volta para o Dashboard via Redis Pub/Sub ou API interna
    // O WebApp pega isso e manda via SignalR para o browser
    await apiInternalClient.PostAsJsonAsync($"/api/callbacks/sandbox/{msg.ConnectionId}", new { text = result.Content });
}
*/