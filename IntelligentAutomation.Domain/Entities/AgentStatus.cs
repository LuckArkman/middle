using IntelligentAutomation.Domain.Enums;

namespace IntelligentAutomation.Domain.Entities;

public class Agent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string DefinitionJson { get; set; } = string.Empty; // Armazena a lógica do agente (triggers, módulos, etc.)
    public AgentStatus Status { get; set; }
    public string UserId { get; set; } // Chave estrangeira para o Usuário
}