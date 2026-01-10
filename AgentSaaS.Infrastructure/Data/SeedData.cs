using AgentSaaS.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver.Linq;

namespace AgentSaaS.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        // 1. Garante Roles
        string[] roles = { "SuperAdmin", "TenantAdmin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2. Garante Tenant "Sistema" (Para o SuperAdmin)
        var sysTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Name == "System");
        if (sysTenant == null)
        {
            sysTenant = new Tenant 
            { 
                Name = "System", 
                PlanType = "Enterprise", 
                MaxAgents = 999 
            };
            context.Tenants.Add(sysTenant);
            await context.SaveChangesAsync();
        }

        // 3. Garante Usuário SuperAdmin
        var adminEmail = "admin@agentsaas.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                TenantId = sysTenant.Id,
                FullName = "System Administrator",
                IsTenantOwner = true,
                EmailConfirmed = true
            };
            
            await userManager.CreateAsync(admin, "Admin123!"); // Senha forte em prod!
            await userManager.AddToRoleAsync(admin, "SuperAdmin");
        }
    }
}