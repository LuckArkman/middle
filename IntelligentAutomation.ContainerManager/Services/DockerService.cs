using Docker.DotNet;
using Docker.DotNet.Models;
using IntelligentAutomation.ContainerManager.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace IntelligentAutomation.ContainerManager.Services;

public class DockerService : IDockerService
{
    private readonly DockerClient _client;
    private readonly ILogger<DockerService> _logger;
    private readonly IConfiguration _configuration;

    public DockerService(ILogger<DockerService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        // Assume Docker is running locally (Unix socket or Windows pipe)
        string dockerUri = Environment.OSVersion.Platform == PlatformID.Win32NT
            ? "npipe://./pipe/docker_engine"
            : "unix:///var/run/docker.sock";

        _client = new DockerClientConfiguration(new Uri(dockerUri)).CreateClient();
    }

    public async Task<string> CreateAgentContainerAsync(Guid agentId, string imageTag)
    {
        _logger.LogInformation("Criando container para o agente {AgentId} usando a imagem {ImageTag}", agentId, imageTag);

        var orchestratorUrl = _configuration["OrchestratorUrl"] ?? "http://host.docker.internal:5001";
        var networkName = _configuration["AgentNetwork"] ?? "ia-network";

        var parameters = new CreateContainerParameters
        {
            Image = imageTag,
            Name = $"agent-{agentId}",
            Env = new List<string>
            {
                $"AGENT_ID={agentId}",
                $"ORCHESTRATOR_URL={orchestratorUrl}"
            },
            HostConfig = new HostConfig
            {
                Memory = 256 * 1024 * 1024, // Limite de 256MB conforme roadmap (Isolamento de recursos)
                CPUCount = 1,
                NetworkMode = networkName // Ensures the container joins the custom network (bridge)
            },
            NetworkingConfig = new NetworkingConfig
            {
                EndpointsConfig = new Dictionary<string, EndpointSettings>
                {
                    { networkName, new EndpointSettings() }
                }
            }
        };

        var response = await _client.Containers.CreateContainerAsync(parameters);
        return response.ID;
    }

    public async Task StartContainerAsync(string containerId)
    {
        _logger.LogInformation("Iniciando container {ContainerId}", containerId);
        await _client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
    }

    public async Task StopContainerAsync(string containerId)
    {
        _logger.LogInformation("Parando container {ContainerId}", containerId);
        await _client.Containers.StopContainerAsync(containerId, new ContainerStopParameters { WaitBeforeKillSeconds = 10 });
    }

    public async Task RemoveContainerAsync(string containerId)
    {
        _logger.LogInformation("Removendo container {ContainerId}", containerId);
        await _client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters { Force = true });
    }
}
