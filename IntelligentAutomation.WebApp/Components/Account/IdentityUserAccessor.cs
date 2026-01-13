using System.Security.Claims;
using IntelligentAutomation.WebApp.Data;
using Microsoft.AspNetCore.Identity;

namespace IntelligentAutomation.WebApp.Components.Account;

internal sealed class IdentityUserAccessor(UserManager<ApplicationUser> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            // Impede que usuários não autenticados acessem endpoints de dados de usuário
            redirectManager.RedirectToWithStatus("Account/InvalidUser", "Error: Unable to load user.");
        }

        return user;
    }
}