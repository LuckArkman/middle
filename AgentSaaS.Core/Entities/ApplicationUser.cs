using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    // O usuário pertence a um Tenant
    public Guid TenantId { get; set; }
    public string FullName { get; set; }
    
    // Flag para saber se é o dono da conta (Admin do Tenant)
    public bool IsTenantOwner { get; set; }
}