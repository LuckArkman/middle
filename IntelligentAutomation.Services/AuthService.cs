using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Dtos;
using IntelligentAutomation.Infrastructure.Persistence;
using IntelligentAutomation.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace IntelligentAutomation.Services;

public class AuthService : IAuthService
{
    private readonly MongoDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IConfiguration _configuration;
    private readonly ITenantService _tenantService;

    public AuthService(
        MongoDbContext context,
        IPasswordService passwordService,
        IConfiguration configuration,
        ITenantService tenantService)
    {
        _context = context;
        _passwordService = passwordService;
        _configuration = configuration;
        _tenantService = tenantService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();

        if (user == null || !_passwordService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new Exception("E-mail ou senha inválidos.");
        }

        _tenantService.SetTenantId(user.TenantId);

        return new LoginResponseDto
        {
            Token = GenerateJwtToken(user),
            Email = user.Email
        };
    }

    public async Task<bool> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _context.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            throw new Exception("Usuário já cadastrado.");
        }

        // Criação de Tenant Automática para novos registros (SaaS Self-Service)
        var tenantIdentifer = request.Email.Split('@')[0].ToLower().Replace(".", "-");

        // Em Mongo, não temos transações cross-collection tão simples sem setup, 
        // mas vamos inserir sequencialmente.

        var tenant = new Tenant
        {
            Name = $"{request.Email} Tenant",
            Identifier = tenantIdentifer
        };

        // Nota: Tenant não está em MongoDbContext. Vamos adicionar ou usar IMongoDatabase.
        // Na verdade, MongoDbContext.cs não tem Tenants. Vou adicionar depois.
        // Por enquanto, vou assumir que existe.
        await _context.Tenants.InsertOneAsync(tenant);

        _passwordService.CreatePasswordHash(request.Password, out string hash, out byte[] salt);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            TenantId = tenant.Id.ToString(),
            Roles = new List<string> { "Admin" }
        };

        await _context.Users.InsertOneAsync(user);

        return true;
    }

    public string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "ChaveSuperSecretaPadraoParaDesenvolvimento123!");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("TenantId", user.TenantId),
            new Claim(ClaimTypes.Role, string.Join(",", user.Roles))
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
