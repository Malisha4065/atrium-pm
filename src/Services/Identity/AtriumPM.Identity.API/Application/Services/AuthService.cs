using AtriumPM.Identity.API.Application.DTOs;
using AtriumPM.Identity.API.Application.Interfaces;
using AtriumPM.Identity.API.Domain.Entities;
using AtriumPM.Identity.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AtriumPM.Identity.API.Application.Services;

/// <summary>
/// Handles authentication: login with BCrypt password verification, 
/// refresh token rotation, and token revocation.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IdentityDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IdentityDbContext db,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<TokenResponse?> LoginAsync(LoginRequest request)
    {
        // Login is cross-tenant — query without tenant filter
        var user = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u =>
                u.TenantId == request.TenantId &&
                u.Email == request.Email.ToLowerInvariant() &&
                u.IsActive);

        if (user is null)
        {
            _logger.LogWarning("Login failed for {Email} under tenant {TenantId}",
                request.Email, request.TenantId);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid password for {Email} under tenant {TenantId}",
                request.Email, request.TenantId);
            return null;
        }

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshTokenStr = _jwtTokenService.GenerateRefreshToken();

        // Persist refresh token
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = user.TenantId,
            Token = refreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        _logger.LogInformation("User {Email} logged in under tenant {TenantId}", user.Email, user.TenantId);

        return new TokenResponse(accessToken, refreshTokenStr, _jwtTokenService.GetAccessTokenExpiry());
    }

    public async Task<TokenResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var existingToken = await _db.RefreshTokens
            .IgnoreQueryFilters()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt =>
                rt.Token == request.RefreshToken &&
                rt.TenantId == request.TenantId &&
                !rt.IsRevoked &&
                rt.ExpiresAt > DateTime.UtcNow);

        if (existingToken is null)
            return null;

        // Revoke the old refresh token (rotation)
        existingToken.IsRevoked = true;

        // Generate new tokens
        var user = existingToken.User;
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshTokenStr = _jwtTokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = user.TenantId,
            Token = newRefreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync();

        return new TokenResponse(accessToken, newRefreshTokenStr, _jwtTokenService.GetAccessTokenExpiry());
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, Guid tenantId)
    {
        var token = await _db.RefreshTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(rt =>
                rt.Token == refreshToken &&
                rt.TenantId == tenantId &&
                !rt.IsRevoked);

        if (token is null) return false;

        token.IsRevoked = true;
        await _db.SaveChangesAsync();
        return true;
    }
}
