using IntelligentAutomation.Domain.Enums;

namespace IntelligentAutomation.Domain.Workflow;

public class TriggerNode : BaseNode
{
    public TriggerType Type { get; set; }

    // Configurações para Cron
    public string? CronExpression { get; set; }

    // Configurações para Webhook
    public string? WebhookSecret { get; set; }
}