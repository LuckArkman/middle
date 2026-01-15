using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace IntelligentAutomation.BlazorApp.Account;

internal sealed class IdentityRedirectManager(NavigationManager navigationManager)
{
    public const string StatusCookieName = "Identity.StatusMessage";

    private static readonly CookieBuilder StatusCookieBuilder = new()
    {
        SameSite = SameSiteMode.Strict,
        HttpOnly = true,
        IsEssential = true,
        MaxAge = TimeSpan.FromSeconds(5),
    };

    [DoesNotReturn]
    public void RedirectTo(string? uri)
    {
        uri ??= "";

        if (navigationManager.Uri.Equals(navigationManager.ToAbsoluteUri(uri).AbsoluteUri, StringComparison.OrdinalIgnoreCase))
        {
            throw new NavigationException(uri);
        }

        navigationManager.NavigateTo(uri);
        throw new NavigationException(uri);
    }

    [DoesNotReturn]
    public void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
    {
        var uriWithoutQuery = navigationManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
        var newUri = navigationManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
        RedirectTo(newUri);
    }

    [DoesNotReturn]
    public void RedirectToWithStatus(string uri, string message, HttpContext context)
    {
        context.Response.Cookies.Append(StatusCookieName, message, StatusCookieBuilder.Build(context));
        RedirectTo(uri);
    }

    [DoesNotReturn]
    public void RedirectToCurrentPage() => RedirectTo(navigationManager.Uri);

    [DoesNotReturn]
    public void RedirectToCurrentPageWithStatus(string message, HttpContext context)
    {
        RedirectToWithStatus(navigationManager.Uri, message, context);
    }
}

// Classe de exceção auxiliar necessária pela implementação acima
internal sealed class NavigationException(string location) : Exception($"Redirecting to {location}");