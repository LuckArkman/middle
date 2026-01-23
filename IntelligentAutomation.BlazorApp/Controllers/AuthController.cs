using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Dtos;
using IntelligentAutomation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace IntelligentAutomation.BlazorApp.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IPasswordService _passwordService;
    private readonly IConfiguration _configuration;

    public AuthController(MongoDbContext context, IPasswordService passwordService, IConfiguration configuration)
    {
        _usersCollection = context.Users;
        _passwordService = passwordService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        if (await _usersCollection.Find(u => u.Email == request.Email.ToLower()).AnyAsync())
        {
            return BadRequest(new { message = "O e-mail já está em uso." });
        }

        _passwordService.CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

        var user = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Roles = new List<string> { "User" }
        };

        await _usersCollection.InsertOneAsync(user);
        return Ok(new { message = "Usuário registrado com sucesso." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var user = await _usersCollection.Find(u => u.Email == request.Email.ToLower()).FirstOrDefaultAsync();

        if (user == null || !_passwordService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return Unauthorized(new { message = "Credenciais inválidas." });
        }

        var token = CreateToken(user);
        return Ok(new LoginResponseDto { Token = token, Email = user.Email });
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };

        if (user.Roles != null)
        {
            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        var jwtKey = _configuration["Jwt:Key"] ?? "MinhaChaveSuperSecreta123!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = creds,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [HttpPost("client-login")]
    public async Task<IActionResult> ClientLogin([FromBody] TokenRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest("Token não fornecido.");
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(request.Token);

        var claimsIdentity = new ClaimsIdentity(jwtSecurityToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { IsPersistent = true };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return Ok();
    }
}

public class TokenRequest
{
    public string Token { get; set; } = string.Empty;
}