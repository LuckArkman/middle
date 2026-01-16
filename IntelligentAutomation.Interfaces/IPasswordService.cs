namespace IntelligentAutomation.Interfaces;

public interface IPasswordService
{
    void CreatePasswordHash(string password, out string passwordHash, out byte[] passwordSalt);
    bool VerifyPasswordHash(string password, string storedHash, byte[] storedSalt);
}