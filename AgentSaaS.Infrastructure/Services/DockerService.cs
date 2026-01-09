using Docker.DotNet;
using Docker.DotNet.Models;

public class ContainerService
{
    private readonly DockerClient _client;

    public ContainerService()
    {
        // Conecta ao Socket do Docker (Linux/Windows)
        _client = new DockerClientConfiguration().CreateClient();
    }

    public async Task<string> StartAgentContainerAsync(Guid agentId, string apiKey, string prompt)
    {
        // Fase 6.2: Recursos Isolados
        var hostConfig = new HostConfig
        {
            Memory = 512 * 1024 * 1024, // Limite de 512MB RAM por agente
            CPUQuota = 50000,           // 0.5 CPU
        };

        var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = "agentsaas/runner:latest", // A imagem compilada do projeto AgentRunner
            Name = $"agent-{agentId}",
            Env = new List<string> 
            { 
                $"AGENT_ID={agentId}",
                $"OPENAI_API_KEY={apiKey}",
                $"SYSTEM_PROMPT={prompt}"
            },
            HostConfig = hostConfig
        });

        await _client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters());
        return response.ID;
    }

    public async Task StopContainerAsync(string containerId)
    {
        await _client.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
        await _client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters());
    }
}