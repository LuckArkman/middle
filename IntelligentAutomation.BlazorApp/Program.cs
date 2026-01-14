using IntelligentAutomation.BlazorApp.Components;
using IntelligentAutomation.BlazorApp.Components.Account;
using IntelligentAutomation.BlazorApp.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// --- Configuração da Persistência do Identity com MongoDB ---
var mongoDbIdentityConfig = new MongoDbIdentityConfiguration
{
    MongoDbSettings = new MongoDbSettings
    {
        ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection"),
        DatabaseName = "SaaS_Automation" // Garanta que este seja o nome do seu banco de dados
    },
    IdentityOptionsAction = options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        // Outras opções de Identity...
    }
};

// --- Registro de Serviços ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- CORREÇÃO PRINCIPAL: Registro do Identity com o provedor MongoDB ---
builder.Services.AddMongoDbIdentity<ApplicationUser, MongoIdentityRole<Guid>>(mongoDbIdentityConfig)
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Registra todos os serviços da aplicação
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor(); // Essencial para o IdentityRedirectManager

// Registra as classes auxiliares do Identity que criamos manualmente
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddSingleton<LoadingService>();
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddTransient<LoadingHandler>();

// Cliente de API com seus handlers
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000"); // URL do API Gateway
})
.AddHttpMessageHandler<AuthHeaderHandler>()
.AddHttpMessageHandler<LoadingHandler>();

// Configuração da Autenticação via Cookies
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// --- Construção e Pipeline da Aplicação ---
var app = builder.Build();

// Seeding do banco de dados do Identity (opcional, mas recomendado)
using (var scope = app.Services.CreateScope())
{
    // Verifique se a classe DbInitializer existe e está correta
    // await DbInitializer.Initialize(scope.ServiceProvider);
}

// Configuração do pipeline de requisição
if (app.Environment.IsDevelopment())
{
    // app.UseMigrationsEndPoint(); // Este é do EF Core, não é mais necessário
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();