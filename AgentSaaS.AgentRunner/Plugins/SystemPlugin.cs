using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace AgentSaaS.AgentRunner.Plugins;

public class SystemPlugin
{
    [KernelFunction, Description("Retorna a data e hora atual do sistema.")]
    public string GetCurrentTime()
    {
        return DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }

    [KernelFunction, Description("Realiza uma pesquisa simulada na internet.")]
    public string WebSearch([Description("O termo a ser pesquisado")] string query)
    {
        Console.WriteLine($"[TOOL USE] Pesquisando por: {query}...");
        // Aqui você integraria com Bing Search API ou Google Search API
        return $"Resultados simulados para '{query}': O .NET 8 é a versão atual com suporte LTS...";
    }
}