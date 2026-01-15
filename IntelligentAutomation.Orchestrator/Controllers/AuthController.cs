using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IntelligentAutomation.Orchestrator.Controllers;

[ApiController]
// ---- INÍCIO DA CORREÇÃO DEFINITIVA ----
// A rota agora é explícita e não depende do nome da classe.
[Route("auth")]
// ---- FIM DA CORREÇÃO DEFINITIVA ----
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

    [HttpPost("register")] // Rota final será: POST /auth/register
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
            Roles = { "User" }
        };

        await _usersCollection.InsertOneAsync(user);
        return Ok(new { message = "Usuário registrado com sucesso." });
    }

    [HttpPost("login")] // Rota final será: POST /auth/login
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var user = await _usersCollection.Find(u => u.Email == request.Email.ToLower()).FirstOrDefaultAsync();

        if (user == null || !_passwordService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return Unauthorized(new { message = "Credenciais inválidas." });
        }

        var token = CreateToken(user);
        return Ok(new LoginResponseDto { Token = token });
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Chave JWT não configurada.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var claimsIdentity = new ClaimsIdentity(claims);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = creds,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}