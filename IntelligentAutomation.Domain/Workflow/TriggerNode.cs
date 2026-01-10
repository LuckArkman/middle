using IntelligentAutomation.Domain.Enums;

namespace IntelligentAutomation.Domain.Workflow;

public class TriggerNode : BaseNode
{
    public TriggerType Type { get; set; }
    public string? CronExpression { get; set; } // Para gatilhos agendados
}