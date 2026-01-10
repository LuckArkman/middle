namespace IntelligentAutomationSaaS.Domain.Entities;

public enum AgentStatus
{
    Created,
    Running,
    Paused,
    Stopped,
    Error
}

public class Agent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string DefinitionJson { get; set; } = string.Empty; // Armazena a lógica do agente (triggers, módulos, etc.)
    public AgentStatus Status { get; set; }
    public Guid UserId { get; set; } // Chave estrangeira para o Usuário
}