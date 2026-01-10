namespace AgentSaaS.Core.Entities;

public class AgentTemplate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string SystemPromptTemplate { get; set; }
    public string RequiredPlugins { get; set; }
    public string IconUrl { get; set; }
    public bool IsPublic { get; set; } = true;
}