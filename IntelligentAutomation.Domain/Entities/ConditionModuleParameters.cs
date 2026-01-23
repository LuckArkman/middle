using IntelligentAutomation.Domain.Workflow;

namespace IntelligentAutomation.Domain.Entities;

public class ConditionModuleParameters : BaseModuleParameters
{
    public string VariableName { get; set; } = string.Empty; // Nome da variável no contexto do workflow (ex: last_http_status)
    public string Operator { get; set; } = "Equals"; // Equals, NotEquals, GreaterThan, LessThan, Contains
    public string ComparisonValue { get; set; } = string.Empty;
}
