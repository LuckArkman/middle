using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(options => {
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();
app.Use(async (context, next) =>
{
    var host = context.Request.Host.Host;
    
    // Se não for o seu domínio principal
    if (host != "meusaas.com" && host != "localhost")
    {
        // Busca no Redis qual Tenant é dono desse domínio
        var tenantId = await redis.StringGetAsync($"domain_map:{host}");
        
        if (!tenantId.IsNullOrEmpty)
        {
            // Injeta o ID do Tenant no Header para o WebApp saber quem é
            context.Request.Headers.Add("X-Tenant-ID", tenantId.ToString());
        }
        else
        {
            context.Response.StatusCode = 404; // Domínio não cadastrado
            return;
        }
    }
    await next();
});
app.UseRateLimiter();
app.MapReverseProxy();

app.Run();