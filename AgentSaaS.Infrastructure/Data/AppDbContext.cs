using AgentSaaS.Core.Entities;
using AgentSaaS.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgentSaaS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly Guid _currentTenantId; // Injetado via Header/Token

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider) 
        : base(options)
    {
        _currentTenantId = tenantProvider.GetTenantId();
    }

    public DbSet<Agent> Agents { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<AgentLog> AgentLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Global Query Filter: Garante que um Tenant nunca veja dados de outro
        modelBuilder.Entity<Agent>().HasQueryFilter(a => a.TenantId == _currentTenantId);
        
        base.OnModelCreating(modelBuilder);
    }
}