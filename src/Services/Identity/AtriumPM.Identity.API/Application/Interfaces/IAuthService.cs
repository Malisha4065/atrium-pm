using AtriumPM.Identity.API.Application.DTOs;

namespace AtriumPM.Identity.API.Application.Interfaces;

public interface IAuthService
{
    Task<TokenResponse?> LoginAsync(LoginRequest request);
    Task<TokenResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> RevokeTokenAsync(string refreshToken, Guid tenantId);
}
