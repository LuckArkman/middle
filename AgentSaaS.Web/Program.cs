using AgentSaaS.Web.Extensions;
using AgentSaaS.Web.Hubs;
using AgentSaaS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// === 1. Configuração de Serviços (DI) ===
builder.Services.AddControllersWithViews();
builder.Services.AddDatabaseLayer(builder.Configuration); // Nossa extensão
builder.Services.AddMessagingLayer(builder.Configuration); // Nossa extensão
builder.Services.AddApplicationServices();                 // Nossa extensão

// Configuração de Observabilidade (OpenTelemetry) - Fase 7
builder.Logging.AddOpenTelemetry(logging => { /* configs... */ });

var app = builder.Build();

// === 2. Pipeline de Requisição (Middleware) ===

// Auto-Migration em Desenvolvimento (Day 1 Feature)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Aguarda DB subir (Resiliência simples)
        await db.Database.MigrateAsync();
        // SeedData.Initialize(scope.ServiceProvider); // Cria usuário Admin padrão
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Importante: Autenticação antes de Autorização
app.UseAuthentication();
app.UseAuthorization();

// Middleware customizado de Tenant (se não usar via DI no DbContext)
// app.UseMiddleware<TenantResolutionMiddleware>(); 

// === 3. Endpoints ===

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseExceptionHandler();
// Endpoint do SignalR
app.MapHub<AgentHub>("/agentHub");

app.Run();