using IntelligentAutomation.BlazorApp.Data;
using Microsoft.AspNetCore.Identity;

namespace IntelligentAutomation.BlazorApp.Components.Account;

internal sealed class IdentityUserAccessor(UserManager<ApplicationUser> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/InvalidUser", "Error: Unable to load user.", context);
        }

        return user;
    }
}