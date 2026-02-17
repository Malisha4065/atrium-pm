using AtriumPM.Identity.API.Domain.Entities;

namespace AtriumPM.Identity.API.Application.Interfaces;

/// <summary>
/// Generates and validates JWT access and refresh tokens.
/// </summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    DateTime GetAccessTokenExpiry();
}
