using AgentSaaS.Infrastructure.Data;
using AgentSaaS.Infrastructure.Interfaces;
using AgentSaaS.Infrastructure.Services;
using AgentSaaS.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace AgentSaaS.Web.Extensions;

public static class ServiceCollectionExtensions
{
    // 1. Configuração do Banco de Dados e Identity
    public static IServiceCollection AddDatabaseLayer(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("AgentSaaS.Infrastructure")));

        // Identity customizado para Multi-tenant
        services.AddIdentity<ApplicationUser, IdentityRole>(options => 
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.SignIn.RequireConfirmedAccount = false; // Facilita MVP
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    // 2. Configuração do Redis e Filas
    public static IServiceCollection AddMessagingLayer(this IServiceCollection services, IConfiguration config)
    {
        var redisConn = config.GetConnectionString("Redis");
        
        // Singleton para conexão Redis (Thread-safe)
        services.AddSingleton<IConnectionMultiplexer>(sp => 
            ConnectionMultiplexer.Connect(redisConn));

        // SignalR com Backplane Redis (para escalar horizontalmente depois)
        services.AddSignalR()
                .AddStackExchangeRedis(redisConn, options => {
                    options.Configuration.ChannelPrefix = "AgentSaaS";
                });

        return services;
    }

    // 3. Serviços do Core (Regras de Negócio)
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, HttpTenantProvider>();
        services.AddScoped<UsageService>();
        services.AddScoped<EncryptionService>();
        
        // Serviços de Infraestrutura
        services.AddScoped<DockerService>(); 
        services.AddScoped<StripeService>();
        
        return services;
    }
}