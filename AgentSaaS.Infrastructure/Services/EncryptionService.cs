using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace AgentSaaS.Infrastructure.Services;

public class EncryptionService
{
    // Em produção, essa chave mestra vem de Environment Variable ou Azure Key Vault
    private readonly string _masterKey; 

    public EncryptionService(IConfiguration config)
    {
        _masterKey = config["Security:MasterKey"]; // 32 chars string
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_masterKey);
        aes.GenerateIV();

        var iv = Convert.ToBase64String(aes.IV);
        
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        var encryptedContent = Convert.ToBase64String(ms.ToArray());
        // Formato: IV:ConteudoCriptografado
        return $"{iv}:{encryptedContent}";
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        var parts = cipherText.Split(':');
        var iv = Convert.FromBase64String(parts[0]);
        var content = Convert.FromBase64String(parts[1]);

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_masterKey);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(content);
        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
        using (var sr = new StreamReader(cs))
        {
            return sr.ReadToEnd();
        }
    }
}