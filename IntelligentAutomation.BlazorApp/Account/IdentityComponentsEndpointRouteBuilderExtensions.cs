using System.Security.Claims;
using System.Text.Json;
using IntelligentAutomation.BlazorApp.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentAutomation.BlazorApp.Account;

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
            var navigationManager = context.RequestServices.GetRequiredService<NavigationManager>();
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
        
        return accountGroup;
    }
}