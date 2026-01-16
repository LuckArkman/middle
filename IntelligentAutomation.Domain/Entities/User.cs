namespace IntelligentAutomation.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public byte[] PasswordSalt { get; set; } = [];
    public List<string> Roles { get; set; } = [];
    public string? CurrentSubscriptionId { get; set; } 
}