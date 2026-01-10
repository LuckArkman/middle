namespace IntelligentAutomation.Application.Dtos;

public class ProvisionAgentRequest
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    // Futuramente, podemos incluir aqui a lista de módulos, recursos de CPU/RAM, etc.
    public Dictionary<string, string> Modules { get; set; } = new();
}