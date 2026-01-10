using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using StackExchange.Redis;
using System.Text.Json;

// 1. Configuração Inicial (Lendo Variáveis de Ambiente injetadas pelo Docker)
var agentId = Environment.GetEnvironmentVariable("AGENT_ID") ?? throw new Exception("AGENT_ID não definido");
var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("API Key não definida");
var redisConnString = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "redis:6379";
var systemPrompt = Environment.GetEnvironmentVariable("SYSTEM_PROMPT") ?? "Você é um assistente útil.";

Console.WriteLine($"[INIT] Iniciando Agente {agentId} (Tenant: {tenantId})...");

// 2. Conexão com Redis (Fila de Entrada e Saída)
var redis = await ConnectionMultiplexer.ConnectAsync(redisConnString);
var db = redis.GetDatabase();
var inboxKey = $"inbox:{agentId}";
var logsChannel = $"logs:{agentId}";

// 3. Configuração do Semantic Kernel
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion("gpt-4", apiKey); // Ou gpt-3.5-turbo para testes baratos

// Carregar Plugins (Fase 6.3 - Dinâmico)
var enabledPlugins = Environment.GetEnvironmentVariable("ENABLED_PLUGINS")?.Split(',') ?? Array.Empty<string>();
// if (enabledPlugins.Contains("WhatsApp")) builder.Plugins.AddFromType<WhatsAppPlugin>();

var kernel = builder.Build();
var chatService = kernel.GetRequiredService<IChatCompletionService>();

// 4. Loop Principal de Execução
Console.WriteLine($"[READY] Agente aguardando mensagens em '{inboxKey}'...");

while (true)
{
    try
    {
        // Bloqueia e aguarda mensagem (Timeout de 5s para não travar eternamente)
        var rawMessage = await db.ListLeftPopAsync(inboxKey);

        if (rawMessage.HasValue)
        {
            Console.WriteLine($"[MSG] Recebida: {rawMessage}");
            await PublishLogAsync(db, logsChannel, "INFO", "Processando mensagem...");

            // Desserializa mensagem (assumindo JSON simples por enquanto)
            // Em prod: Use a classe AgentInboxMessage definida anteriormente
            string userMessage = rawMessage.ToString(); 

            // Configura o Histórico (Stateless por enquanto, ou carregue do DB aqui)
            var history = new ChatHistory(systemPrompt);
            history.AddUserMessage(userMessage);

            // Executa a IA
            var result = await chatService.GetChatMessageContentAsync(history, kernel: kernel);

            Console.WriteLine($"[IA] Resposta: {result.Content}");
            
            // Publica resposta no canal de Logs (para o SignalR pegar)
            await PublishLogAsync(db, logsChannel, "RESPONSE", result.Content);
            
            // TODO: Aqui você enviaria para o WhatsApp/Webhook de saída
        }
        else
        {
            await Task.Delay(1000); // Polling suave
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[ERROR] {ex.Message}");
        await PublishLogAsync(db, logsChannel, "ERROR", ex.Message);
        await Task.Delay(5000); // Backoff em caso de erro
    }
}

// Helper para enviar logs estruturados para o Redis (SignalR consome isso)
static async Task PublishLogAsync(IDatabase db, string channel, string level, string message)
{
    var logPayload = JsonSerializer.Serialize(new 
    { 
        Timestamp = DateTime.UtcNow, 
        Level = level, 
        Message = message 
    });
    await db.PublishAsync(channel, logPayload);
}