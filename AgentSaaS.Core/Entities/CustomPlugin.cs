namespace AgentSaaS.Core.Entities;

public class CustomPlugin
{
    public int Id { get; set; }
    public Guid AgentId { get; set; }
    public string Name { get; set; } // Ex: "Meu CRM"
    public string OpenApiSpecUrl { get; set; } // URL do Swagger JSON
    public string AuthHeader { get; set; } // "Bearer xxxxx"
}