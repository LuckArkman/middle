using IntelligentAutomation.Domain.Enums;

namespace IntelligentAutomation.Domain.Entities;

public class Agent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string DefinitionJson { get; set; } = string.Empty; // Lógica de workflow (nós/conexões)
    public AgentStatus Status { get; set; }
    public string UserId { get; set; }

    // Fase 5.2 - Configurações Avançadas
    public string LlmType { get; set; } = "GPT-4o"; // Ex: GPT-4, Claude-3, etc.
    public bool EnableContextMemory { get; set; } = true;
    public string? McpConfiguration { get; set; } // Configuração Model Context Protocol
    public List<string> AreasOfExpertise { get; set; } = new();

    // Recursos de Infraestrutura
    public long MemoryLimitMb { get; set; } = 256;
    public float CpuLimit { get; set; } = 0.5f;
}
