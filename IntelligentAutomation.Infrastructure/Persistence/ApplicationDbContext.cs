using IntelligentAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelligentAutomation.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Agent> Agents { get; set; }
    // DbSet para User e Plan serão adicionados futuramente

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações do modelo (constraints, índices, etc.) podem ser adicionadas aqui.
        modelBuilder.Entity<Agent>().HasKey(a => a.Id);
        modelBuilder.Entity<Agent>().Property(a => a.Name).IsRequired().HasMaxLength(100);
    }
}