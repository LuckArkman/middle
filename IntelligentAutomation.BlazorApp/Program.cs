using IntelligentAutomation.BlazorApp.Components;
using IntelligentAutomation.BlazorApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// --- Configuração de Serviços ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddHttpClient<ApiClient>(client =>
    {
        client.BaseAddress = new Uri("http://localhost:5000");
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

app.Run();