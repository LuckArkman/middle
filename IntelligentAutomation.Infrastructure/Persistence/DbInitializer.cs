using IntelligentAutomation.Domain.Entities;
using MongoDB.Driver;

namespace IntelligentAutomation.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task Initialize(MongoDbContext context)
    {
        // Seed Plans
        var planCount = await context.Plans.CountDocumentsAsync(_ => true);
        if (planCount == 0)
        {
            var plans = new List<Plan>
            {
                new Plan
                {
                    Name = "Básico",
                    MaxAgents = 2,
                    MonthlyPrice = 49.90m,
                    Description = "Ideal para iniciantes",
                    TenantId = "system"
                },
                new Plan
                {
                    Name = "Intermediário",
                    MaxAgents = 5,
                    MonthlyPrice = 99.90m,
                    Description = "Para pequenos negócios",
                    TenantId = "system"
                },
                new Plan
                {
                    Name = "Pró",
                    MaxAgents = 10,
                    MonthlyPrice = 199.90m,
                    Description = "Uso profissional intenso",
                    EnableAdvancedLLMs = true,
                    TenantId = "system"
                },
                new Plan
                {
                    Name = "Empresarial",
                    MaxAgents = 20,
                    MonthlyPrice = 499.90m,
                    Description = "Controle total e escala",
                    EnableAdvancedLLMs = true,
                    EnableCustomIntegrations = true,
                    TenantId = "system"
                }
            };

            await context.Plans.InsertManyAsync(plans);
        }

        // Seed Default Tenant
        var tenantCount = await context.Tenants.CountDocumentsAsync(_ => true);
        if (tenantCount == 0)
        {
            var defaultTenant = new Tenant
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "Default Tenant",
                Identifier = "default",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Tenants.InsertOneAsync(defaultTenant);
        }
    }
}
