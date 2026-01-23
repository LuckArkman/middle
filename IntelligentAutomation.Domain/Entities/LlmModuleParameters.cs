using IntelligentAutomation.Domain.Workflow;

namespace IntelligentAutomation.Domain.Entities;

public class LlmModuleParameters : BaseModuleParameters
{
    public string Model { get; set; } = "gpt-3.5-turbo"; // ou gpt-4
    public string SystemPrompt { get; set; } = "Você é um assistente útil.";
    public string UserPromptTemplate { get; set; } = string.Empty; // Ex: "Analise o seguinte dado: {{node_id}}"
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 500;
}
