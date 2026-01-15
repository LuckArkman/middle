namespace IntelligentAutomation.Dtos;

public class CreateAgentDto
{
    public string Name { get; set; } = string.Empty;
    public string DefinitionJson { get; set; } = "{}";
}