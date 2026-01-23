using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Dtos;

namespace IntelligentAutomation.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<bool> RegisterAsync(RegisterRequestDto request);
    string GenerateJwtToken(User user);
}
