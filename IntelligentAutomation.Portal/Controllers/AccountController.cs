using IntelligentAutomation.Dtos;
using IntelligentAutomation.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IntelligentAutomation.Infrastructure.Persistence;
using MongoDB.Driver;

namespace IntelligentAutomation.Portal.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly MongoDbContext _db;
    private readonly IPasswordService _passwordService;

    public AccountController(IAuthService authService, MongoDbContext db, IPasswordService passwordService)
    {
        _authService = authService;
        _db = db;
        _passwordService = passwordService;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        if (!ModelState.IsValid) return View(request);

        try
        {
            var user = await _db.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();

            if (user == null || !_passwordService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError("", "Credenciais inválidas.");
                return View(request);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("TenantId", user.TenantId),
                new Claim(ClaimTypes.Role, string.Join(",", user.Roles))
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(request);
        }
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        try
        {
            await _authService.RegisterAsync(request);
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(request);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
