namespace AgentSaaS.Core.Entities
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public int MaxAgents { get; set; }
        public string Name { get; set; }
        public string PlanType { get; set; } // Basico, Intermediario, Pro, Enterprise
        public string? CustomDomain { get; set; } // ex: "ia.cliente.com"
        public string BrandColor { get; set; } = "#0d6efd"; // Bootstrap Primary Default
        public string LogoUrl { get; set; }
        public string CssOverride { get; set; } // CSS Customizado
    }
}