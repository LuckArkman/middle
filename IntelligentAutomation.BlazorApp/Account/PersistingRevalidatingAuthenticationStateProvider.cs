using System.Diagnostics;
using System.Security.Claims;
using IntelligentAutomation.BlazorApp.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IntelligentAutomation.BlazorApp.Account;

public sealed class PersistingRevalidatingAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PersistentComponentState _state;
    private readonly IOptions<IdentityOptions> _options;

    private readonly PersistingComponentStateSubscription _subscription;

    private Task<AuthenticationState>? _authenticationStateTask;

    public PersistingRevalidatingAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory,
        PersistentComponentState state,
        IOptions<IdentityOptions> options)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _state = state;
        _options = options;

        AuthenticationStateChanged += OnAuthenticationStateChanged;
        _subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveServer);
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _authenticationStateTask ??= GetAuthenticationStateAsyncCore();

    private async Task<AuthenticationState> GetAuthenticationStateAsyncCore()
    {
        try
        {
            // Tenta obter o estado do componente persistido primeiro.
            var isPersistent = _state.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo);

            // Se não encontrou no estado persistido, ou se o usuário não estava logado, usa a implementação base (que consulta o cookie).
            if (!isPersistent || userInfo is null)
            {
                return await base.GetAuthenticationStateAsync();
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new(GetClaimType(nameof(userInfo.UserId)), userInfo.UserId),
                    new(GetClaimType(nameof(userInfo.Email)), userInfo.Email),
                ],
                IdentityConstants.ApplicationScheme));
            
            return new AuthenticationState(principal);
        }
        catch
        {
            // Se algo der errado, retorna um usuário anônimo.
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
    protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        // Obtém o UserManager do escopo de dependência para evitar o uso de serviços com escopo de longa duração.
        await using var scope = _scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return await ValidateSecurityStampAsync(userManager, authenticationState.User);
    }

    private async Task<bool> ValidateSecurityStampAsync(UserManager<ApplicationUser> userManager, ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return false;
        }
        else if (!userManager.SupportsUserSecurityStamp)
        {
            return true;
        }
        else
        {
            var principalStamp = principal.FindFirstValue(_options.Value.ClaimsIdentity.SecurityStampClaimType);
            var userStamp = await userManager.GetSecurityStampAsync(user);
            return principalStamp == userStamp;
        }
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        _authenticationStateTask = task;
    }

    private async Task OnPersistingAsync()
    {
        if (_authenticationStateTask is null)
        {
            throw new UnreachableException($"Authentication state not set in {nameof(OnPersistingAsync)}().");
        }

        var authenticationState = await _authenticationStateTask;
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            var userId = principal.FindFirstValue(GetClaimType(nameof(UserInfo.UserId)));
            var email = principal.FindFirstValue(GetClaimType(nameof(UserInfo.Email)));

            if (userId != null && email != null)
            {
                _state.PersistAsJson(nameof(UserInfo), new UserInfo { UserId = userId, Email = email });
            }
        }
    }
    
    private string GetClaimType(string claimName)
    {
        return claimName switch
        {
            nameof(UserInfo.UserId) => _options.Value.ClaimsIdentity.UserIdClaimType,
            nameof(UserInfo.Email) => _options.Value.ClaimsIdentity.EmailClaimType,
            _ => throw new NotSupportedException($"Claim '{claimName}' is not supported."),
        };
    }

    protected override void Dispose(bool disposing)
    {
        _subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
        base.Dispose(disposing);
    }

    [Serializable]
    private sealed class UserInfo
    {
        public required string UserId { get; set; }
        public required string Email { get; set; }
    }
}