using AgentSaaS.Infrastructure.Data;
using AgentSaaS.Core.Entities;
using AgentSaaS.Core.Enums;
using AgentSaaS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace AgentSaaS.Orchestrator;

public class OrchestratorWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DockerService _dockerService;
    private readonly ILogger<OrchestratorWorker> _logger;

    public OrchestratorWorker(IServiceProvider serviceProvider, DockerService dockerService, ILogger<OrchestratorWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _dockerService = dockerService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Orchestrator iniciado. Monitorando agentes...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var encryption = scope.ServiceProvider.GetRequiredService<EncryptionService>(); // Fase 13

                    // === LÓGICA DE START ===
                    // Busca agentes que deveriam estar rodando (Status = Executando) mas não têm ContainerId
                    var agentsToStart = await context.Agents
                        .Where(a => a.Status == AgentStatus.Executando && a.ContainerId == null)
                        .ToListAsync(stoppingToken);

                    foreach (var agent in agentsToStart)
                    {
                        // Descriptografa a chave (Segurança)
                        var realKey = encryption.Decrypt(agent.OpenAiApiKey);
                        
                        // Inicia o container
                        var containerId = await _dockerService.StartAgentAsync(
                            agent.Id, 
                            agent.TenantId, 
                            realKey, 
                            agent.SystemPrompt, 
                            new List<string> { "WhatsApp", "Memory" } // Exemplo: viria do plano
                        );

                        // Atualiza DB
                        agent.ContainerId = containerId;
                        agent.LastActivity = DateTime.UtcNow;
                        await context.SaveChangesAsync(stoppingToken);
                    }

                    // === LÓGICA DE STOP ===
                    // Busca agentes que o usuário mandou parar (Status = Parado) mas ainda têm ContainerId
                    var agentsToStop = await context.Agents
                        .Where(a => a.Status == AgentStatus.Parado && a.ContainerId != null)
                        .ToListAsync(stoppingToken);

                    foreach (var agent in agentsToStop)
                    {
                        await _dockerService.StopAgentAsync(agent.ContainerId);
                        
                        agent.ContainerId = null;
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no ciclo do Orchestrator.");
            }

            // Aguarda 5 segundos antes do próximo ciclo (Polling)
            await Task.Delay(5000, stoppingToken);
        }
    }
}