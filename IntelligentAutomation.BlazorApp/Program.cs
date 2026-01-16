using System.Text;
using IntelligentAutomation.BlazorApp.Components;
using IntelligentAutomation.BlazorApp.Services;
using IntelligentAutomation.Infrastructure.Persistence;
using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// --- Configuração de Serviços ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
builder.Services.AddControllers();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddScoped<IPaymentGatewayService, MercadoPagoService>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };
    });

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = new MongoUrl(builder.Configuration.GetConnectionString("MongoDbConnection")).DatabaseName;
    return client.GetDatabase(databaseName);
});
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddHttpClient<ApiClient>(client =>
    {
        client.BaseAddress = new Uri("https://localhost:7012");
    })
    .AddHttpMessageHandler<AuthHeaderHandler>();

var app = builder.Build();

// --- Configuração do Pipeline de Requisição ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ---- INÍCIO DA CORREÇÃO PRINCIPAL ----
// A ordem do pipeline foi corrigida. UseAntiforgery() agora está DEPOIS de UseAuthentication/UseAuthorization.
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery(); // Posição correta
// ---- FIM DA CORREÇÃO PRINCIPAL ----

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
    
app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});
app.MapControllers();
app.Run();