using AgentSaaS.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSaaS.Infrastructure.Data;

namespace AgentSaaS.Orchestrator
{
    public class OrchestratorWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ContainerService _containerService;

        public OrchestratorWorker(IServiceProvider serviceProvider, ContainerService containerService)
        {
            _serviceProvider = serviceProvider;
            _containerService = containerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // 1. Busca agentes marcados para iniciar mas sem container
                    var agentsToStart = db.Agents
                        .Where(a => a.Status == AgentStatus.Executando && a.ContainerId == null)
                        .ToList();

                    foreach (var agent in agentsToStart)
                    {
                        // Inicia container isolado
                        var containerId = await _containerService.StartAgentContainerAsync(agent.Id, "key_segura", agent.SystemPrompt);

                        agent.ContainerId = containerId;
                        await db.SaveChangesAsync();
                    }

                    // 2. Busca agentes marcados para parar
                    var agentsToStop = db.Agents
                        .Where(a => a.Status == AgentStatus.Parado && a.ContainerId != null)
                        .ToList();

                    foreach (var agent in agentsToStop)
                    {
                        await _containerService.StopContainerAsync(agent.ContainerId);
                        agent.ContainerId = null;
                        await db.SaveChangesAsync();
                    }
                }

                await Task.Delay(5000, stoppingToken); // Loop de verificação
            }
        }
    }
}
