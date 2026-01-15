using System.Security.Cryptography;
using System.Text;
using IntelligentAutomation.Interfaces;

namespace IntelligentAutomation.Services;

public class PasswordService : IPasswordService
{
    public void CreatePasswordHash(string password, out string passwordHash, out byte[] passwordSalt)
    {
        // Usa HMAC-SHA512 para criar um hash seguro com um salt gerado aleatoriamente
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        passwordHash = Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPasswordHash(string password, string storedHash, byte[] storedSalt)
    {
        // Usa o mesmo salt armazenado para recriar o hash e compará-lo com o que está no banco de dados
        using var hmac = new HMACSHA512(storedSalt);
        var computedHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        var computedHash = Convert.ToBase64String(computedHashBytes);
        return computedHash == storedHash;
    }
}