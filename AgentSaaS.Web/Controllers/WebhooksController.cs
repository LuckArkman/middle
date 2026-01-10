using System.Text.Json;
using AgentSaaS.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace AgentSaaS.Web.Controllers;

[Route("api/webhooks")]
[ApiController]
public class WebhooksController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;

    public WebhooksController(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    // Exemplo genérico (adaptável para Twilio/Waha)
    [HttpPost("whatsapp/{agentId}")]
    public async Task<IActionResult> ReceiveWhatsApp(Guid agentId, [FromBody] WhatsAppIncomingPayload payload)
    {
        // 1. Validar se o agente existe (Opcional: Cachear isso para performance)
        
        // 2. Serializar a mensagem para processamento
        var messageData = new AgentInboxMessage
        {
            FromNumber = payload.From,
            Content = payload.Body,
            Timestamp = DateTime.UtcNow
        };

        // 3. Enfileirar no Redis (Inbox do Agente)
        // Chave: "inbox:{agentId}"
        var db = _redis.GetDatabase();
        await db.ListRightPushAsync($"inbox:{agentId}", System.Text.Json.JsonSerializer.Serialize(messageData));

        return Ok();
    }
    
    [HttpGet("whatsapp/{agentId}")]
    public IActionResult VerifyWebhook(Guid agentId, [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        // Valide se o token bate com o que você configurou no Painel da Meta
        // Em produção, busque o token esperado na tabela 'AgentIntegrations'
        const string VERIFY_TOKEN = "meu_token_secreto_saas";

        if (mode == "subscribe" && token == VERIFY_TOKEN)
        {
            Console.WriteLine($"Webhook verificado para Agente {agentId}");
            return Ok(int.Parse(challenge)); // Retorna o challenge puro
        }

        return Forbid();
    }

    [HttpPost("whatsapp/{agentId}")]
    public async Task<IActionResult> ReceiveMessage(Guid agentId, [FromBody] JsonElement payload)
    {
        // A estrutura do JSON da Meta é complexa. 
        // Recomendo criar classes DTO robustas ou navegar pelo JsonElement.
    
        // Exemplo simplificado de extração:
        try 
        {
            var entry = payload.GetProperty("entry")[0];
            var changes = entry.GetProperty("changes")[0];
            var value = changes.GetProperty("value");
        
            if (value.TryGetProperty("messages", out var messages))
            {
                var msg = messages[0];
                var from = msg.GetProperty("from").GetString();
                var text = msg.GetProperty("text").GetProperty("body").GetString();

                // Envia para o Redis (como feito anteriormente)
                await _queueService.EnqueueMessageAsync(agentId, from, text);
            }
        }
        catch 
        {
            // Ignora status updates (SENT, DELIVERED, READ)
            // Apenas mensagens de texto nos interessam agora
        }

        return Ok();
    }
}