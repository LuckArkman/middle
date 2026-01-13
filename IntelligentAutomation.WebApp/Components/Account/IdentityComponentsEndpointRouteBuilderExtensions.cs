using System.Security.Claims;
using System.Text.Json;
using IntelligentAutomation.WebApp.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentAutomation.WebApp.Components.Account;

internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup("/Account");

        accountGroup.MapPost("/PerformExternalLogin", (
            HttpContext context,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromForm] string provider,
            [FromForm] string returnUrl) =>
        {
            // ---- INÍCIO DA CORREÇÃO 1 ----
            // Obtém o NavigationManager do contêiner de DI para construir a URL de redirecionamento
            var navigationManager = context.RequestServices.GetRequiredService<NavigationManager>();
            // ---- FIM DA CORREÇÃO 1 ----

            // Usa 'navigationManager' em vez de 'navigation'
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, navigationManager.ToAbsoluteUri(returnUrl).ToString());
            return TypedResults.Challenge(properties, [provider]);
        });
        
        accountGroup.MapPost("/Logout", async (
            ClaimsPrincipal user,
            SignInManager<ApplicationUser> signInManager,
            [FromForm] string returnUrl) =>
        {
            await signInManager.SignOutAsync();
            return TypedResults.LocalRedirect($"~/{returnUrl}");
        });
        
        var manageGroup = accountGroup.MapGroup("/Manage").RequireAuthorization();
        
        manageGroup.MapPost("/LinkExternalLogin", async (
            HttpContext context,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromForm] string provider) => // <-- PARÂMETRO 'provider' ADICIONADO AQUI
        {
            // ---- FIM DA CORREÇÃO PRINCIPAL ----
            await context.SignOutAsync(IdentityConstants.ExternalScheme);
            
            var navigationManager = context.RequestServices.GetRequiredService<NavigationManager>();
            var redirectUrl = navigationManager.ToAbsoluteUri("Account/Manage/ExternalLogins").ToString();
            
            // A variável 'provider' agora existe e o código é válido
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userManager.GetUserId(context.User));
            return TypedResults.Challenge(properties, [provider]);
        });
        
        var loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var downloadLogger = loggerFactory.CreateLogger("DownloadPersonalData");
        
        manageGroup.MapPost("/DownloadPersonalData", async (
            HttpContext context,
            [FromServices] UserManager<ApplicationUser> userManager) => // <-- Injeta o UserManager
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is null)
            {
                return Results.NotFound($"Unable to load user with ID '{userManager.GetUserId(context.User)}'.");
            }

            var personalData = new Dictionary<string, string>();
            var personalDataProps = typeof(ApplicationUser).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            var logins = await userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            personalData.Add("Authenticator Key", (await userManager.GetAuthenticatorKeyAsync(user))!);
            var fileBytes = JsonSerializer.SerializeToUtf8Bytes(personalData);
            
            context.Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.json");
            return TypedResults.File(fileBytes, contentType: "application/json", fileDownloadName: "PersonalData.json");
        });
        
        return accountGroup;
    }
}