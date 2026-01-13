using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IntelligentAutomation.WebApp.Components;
using IntelligentAutomation.WebApp.Components.Account;
using IntelligentAutomation.WebApp.Data;
using IntelligentAutomation.WebApp.Services;
using Blazor.Diagrams;

var builder = WebApplication.CreateBuilder(args);

// --- Registro de Serviços ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

// Serviços de UI
builder.Services.AddScoped<ToastService>();
builder.Services.AddSingleton<LoadingService>();
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddTransient<LoadingHandler>();

// Cliente de API
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
})
.AddHttpMessageHandler<AuthHeaderHandler>()
.AddHttpMessageHandler<LoadingHandler>();

// Configuração do Identity
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Serviço que estava causando o problema por estar registrado sem seu middleware
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// --- Construção e Pipeline da Aplicação ---
var app = builder.Build();

// Seeding do banco de dados do Identity
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.Initialize(scope.ServiceProvider);
}

// ---- INÍCIO DA CORREÇÃO PRINCIPAL ----
// O pipeline de middleware foi restaurado para a ordem correta e completa.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // Este middleware é necessário se o serviço correspondente estiver registrado
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
// ---- FIM DA CORREÇÃO PRINCIPAL ----

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery(); // Importante que esteja após UseStaticFiles

// Middleware de autenticação na ordem correta
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();