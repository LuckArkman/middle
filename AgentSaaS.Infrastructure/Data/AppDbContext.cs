using AgentSaaS.Core.Entities;
using AgentSaaS.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgentSaaS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Agent> Agents { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    // Outros DbSets...

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração do Tenant
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlanType).HasDefaultValue("Basic");
        });

        // Configuração do Agente
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasOne(a => a.Tenant)
                .WithMany()
                .HasForeignKey(a => a.TenantId);

            // !!! O FILTRO MÁGICO DE MULTI-TENANCY !!!
            // Automaticamente filtra queries pelo TenantId atual
            entity.HasQueryFilter(a => a.TenantId == _tenantProvider.GetTenantId());
        });
    }
    
    // Sobrescreve SaveChanges para injetar TenantId automaticamente
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Agent>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.TenantId = _tenantProvider.GetTenantId();
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}