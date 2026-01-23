using IntelligentAutomation.Dtos;
using IntelligentAutomation.ContainerManager.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IntelligentAutomation.ContainerManager.Controllers;

[ApiController]
[Route("[controller]")]
public class ContainersController : ControllerBase
{
    private readonly ILogger<ContainersController> _logger;
    private readonly IDockerService _dockerService;

    public ContainersController(ILogger<ContainersController> logger, IDockerService dockerService)
    {
        _logger = logger;
        _dockerService = dockerService;
    }

    [HttpPost("{agentId}/start")]
    public async Task<IActionResult> StartContainer(Guid agentId)
    {
        _logger.LogInformation("Solicitado início do container para o Agente ID: {AgentId}", agentId);
        string containerId = $"agent-{agentId}";

        try
        {
            await _dockerService.StartContainerAsync(containerId);
        }
        catch (Exception ex)
        {
            // Se falhar, tenta provisionar e iniciar novamente
            _logger.LogWarning("Falha ao iniciar container {ContainerId}. Tentando provisionar... Erro: {Msg}", containerId, ex.Message);

            try
            {
                string imageTag = "intelligent-automation-agent:latest";
                await _dockerService.CreateAgentContainerAsync(agentId, imageTag);
                await _dockerService.StartContainerAsync(containerId);
            }
            catch (Exception ex2)
            {
                _logger.LogError(ex2, "Falha crítica ao provisionar e iniciar o agente {AgentId}", agentId);
                return StatusCode(500, "Falha ao provisionar/iniciar agente.");
            }
        }

        return Ok(new { AgentId = agentId, Status = "Started" });
    }

    [HttpPost("{agentId}/stop")]
    public async Task<IActionResult> StopContainer(Guid agentId)
    {
        _logger.LogInformation("Solicitada parada do container para o Agente ID: {AgentId}", agentId);
        string containerId = $"agent-{agentId}";

        try { await _dockerService.StopContainerAsync(containerId); } catch { }
        return Ok(new { AgentId = agentId, Status = "Stopped" });
    }

    [HttpDelete("{agentId}")]
    public async Task<IActionResult> DestroyContainer(Guid agentId)
    {
        _logger.LogInformation("Solicitada destruição do container para o Agente ID: {AgentId}", agentId);
        string containerId = $"agent-{agentId}";

        try { await _dockerService.RemoveContainerAsync(containerId); } catch { }
        return NoContent();
    }
}
