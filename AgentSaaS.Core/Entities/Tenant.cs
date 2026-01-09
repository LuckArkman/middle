namespace AgentSaaS.Core.Entities
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public int MaxAgents { get; set; }
        public string Name { get; set; }
        public string PlanType { get; set; } // Basico, Intermediario, Pro, Enterprise
    }
}