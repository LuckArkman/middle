using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace AgentSaaS.AgentRunner.Plugins;

public class WhatsAppPlugin
{
    private readonly HttpClient _httpClient;
    private readonly string _apiEndpoint;
    private readonly string _token;

    public WhatsAppPlugin()
    {
        _httpClient = new HttpClient();
        // Na prática, pegar de variáveis de ambiente injetadas pelo Orchestrator
        _apiEndpoint = Environment.GetEnvironmentVariable("WHATSAPP_API_URL"); 
        _token = Environment.GetEnvironmentVariable("WHATSAPP_TOKEN");
    }

    [KernelFunction, Description("Envia uma mensagem de WhatsApp para um número específico.")]
    public async Task<string> SendMessage(
        [Description("O número de telefone no formato internacional (ex: 5511999999999)")] string phoneNumber,
        [Description("O texto da mensagem a ser enviada")] string message)
    {
        Console.WriteLine($"[TOOL] Enviando WhatsApp para {phoneNumber}: {message}");

        // Exemplo fictício de chamada HTTP
        // var payload = new { to = phoneNumber, text = message };
        // var response = await _httpClient.PostAsJsonAsync(_apiEndpoint, payload);
        
        // Simulação para o MVP
        await Task.Delay(500); // Simula latência de rede
        return $"Sucesso: Mensagem entregue para {phoneNumber}.";
    }
}