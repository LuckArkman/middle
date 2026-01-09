using AgentSaaS.Web.Hubs;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.SignalR;

namespace AgentSaaS.Orchestrator;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
    
    public async Task MonitorContainerLogsAsync(string containerId, string agentId, IHubContext<AgentHub> hubContext)
    {
        var logParameters = new ContainerLogsParameters
        {
            ShowStdout = true,
            ShowStderr = true,
            Follow = true, // Mantém a conexão aberta
            Timestamps = true
        };

        using var stream = await _client.Containers.GetContainerLogsAsync(containerId, false, logParameters);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            // Envia para o Frontend em tempo real via SignalR
            // O Orchestrator precisa de referência ao AgentSaaS.Web ou uma biblioteca compartilhada de Hubs
            await hubContext.Clients.Group(agentId).SendAsync("ReceiveLog", line);
        
            // TODO: Persistir no Banco (PostgreSQL/Elastic) para histórico (Fase 7.2)
        }
    }
}