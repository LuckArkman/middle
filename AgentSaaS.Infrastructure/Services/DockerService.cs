using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace AgentSaaS.Infrastructure.Services;

public class DockerService
{
    private readonly DockerClient _client;
    private readonly ILogger<DockerService> _logger;

    public DockerService(ILogger<DockerService> logger)
    {
        _logger = logger;

        // Detecta o SO para conectar no Socket correto
        var dockerUri = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
            ? new Uri("npipe://./pipe/docker_engine")
            : new Uri("unix:///var/run/docker.sock");

        _client = new DockerClientConfiguration(dockerUri).CreateClient();
    }

    public async Task<string> StartAgentAsync(Guid agentId, Guid tenantId, string apiKey, string systemPrompt, List<string> plugins)
    {
        const string ImageName = "agentsaas/runner:latest";
        var containerName = $"agent-{agentId}";

        try
        {
            // 1. Garante que a imagem existe (Pull se necessário)
            // Nota: Em produção, faça isso no deploy, não na execução, para ser mais rápido.
            await EnsureImageExistsAsync(ImageName);

            // 2. Configurações de Hardware (Isolamento de Recursos - Fase 6)
            var hostConfig = new HostConfig
            {
                // Limite de 512MB de RAM e 0.5 CPU por agente
                Memory = 512 * 1024 * 1024,
                NanoCPUs = 500000000, 
                // Auto-remove: O container se destrói quando o processo termina (limpeza automática)
                AutoRemove = true, 
                // Rede: Conecta na mesma rede do docker-compose para achar o Redis
                NetworkMode = "agentsaas_default" 
            };

            // 3. Criação do Container
            var createParams = new CreateContainerParameters
            {
                Image = ImageName,
                Name = containerName,
                // Injeção de Dependência via Variáveis de Ambiente
                Env = new List<string>
                {
                    $"AGENT_ID={agentId}",
                    $"TENANT_ID={tenantId}",
                    $"OPENAI_API_KEY={apiKey}", // O Runner lerá isso
                    $"SYSTEM_PROMPT={systemPrompt}",
                    $"ENABLED_PLUGINS={string.Join(",", plugins)}",
                    "REDIS_CONNECTION=redis:6379" // Nome do serviço no docker-compose
                },
                HostConfig = hostConfig
            };

            _logger.LogInformation($"Criando container {containerName}...");
            var response = await _client.Containers.CreateContainerAsync(createParams);

            // 4. Inicia o Container
            await _client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters());
            
            _logger.LogInformation($"Container {containerName} iniciado com ID {response.ID.Substring(0, 12)}.");
            
            return response.ID;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Falha ao iniciar agente {agentId}");
            throw;
        }
    }

    public async Task StopAgentAsync(string containerId)
    {
        try 
        {
            _logger.LogInformation($"Parando container {containerId}...");
            // Dá 5 segundos para o agente encerrar graciosamente antes de matar
            await _client.Containers.StopContainerAsync(containerId, new ContainerStopParameters { WaitBeforeKillSeconds = 5 });
        }
        catch (DockerContainerNotFoundException)
        {
            _logger.LogWarning($"Container {containerId} já não existe.");
        }
    }

    private async Task EnsureImageExistsAsync(string imageName)
    {
        var images = await _client.Images.ListImagesAsync(new ImagesListParameters { MatchName = imageName });
        if (images.Count == 0)
        {
            _logger.LogInformation($"Imagem {imageName} não encontrada. Baixando...");
            await _client.Images.CreateImageAsync(
                new ImagesCreateParameters { FromImage = imageName, Tag = "latest" },
                null,
                new Progress<JSONMessage>());
        }
    }
}