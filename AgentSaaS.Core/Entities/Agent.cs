using System.ComponentModel.DataAnnotations;
using AgentSaaS.Core.Enums;

namespace AgentSaaS.Core.Entities
{
    public class Agent
    {

        public string Area { get; set; } // Vendas, Suporte, etc.
        public string? ContainerId { get; set; } // ID do Container Docker
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime LastActivity { get; set; }

        // Configurações do Agente (Fase 5)
        public string LlmModel { get; set; } = "gpt-4-turbo";

        [Required]
        public string Name { get; set; }

        // Controle de Ciclo de Vida (Fase 6)
        public AgentStatus Status { get; set; } = AgentStatus.Parado;
        public string SystemPrompt { get; set; }
        public Tenant Tenant { get; set; }

        // Isolamento Multi-tenant
        public Guid TenantId { get; set; }
    }
}