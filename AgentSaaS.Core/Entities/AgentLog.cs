namespace AgentSaaS.Core.Entities
{
    public class AgentLog
    {
        public Guid AgentId { get; set; }
        public int Id { get; set; }
        public string Level { get; set; } // Info, Warning, Error
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}