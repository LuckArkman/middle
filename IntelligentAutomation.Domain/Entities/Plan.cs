namespace IntelligentAutomation.Domain.Entities;

public class Plan : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Básico, Intermediário, Pró, Empresarial
    public int MaxAgents { get; set; } // 2, 5, 10, 20
    public int MaxAreasPerAgent { get; set; } = 20;
    public decimal MonthlyPrice { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Recursos habilitados (Fase 4.1)
    public bool EnableAdvancedLLMs { get; set; } = false;
    public bool EnableCustomIntegrations { get; set; } = false;
    public bool Enable24hExecution { get; set; } = true;
}