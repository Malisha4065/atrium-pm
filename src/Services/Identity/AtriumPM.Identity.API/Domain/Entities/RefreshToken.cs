using AtriumPM.Shared.Interfaces;

namespace AtriumPM.Identity.API.Domain.Entities;

/// <summary>
/// JWT refresh token for sliding-window session renewal.
/// Implements IMustHaveTenant for automatic tenant scoping.
/// </summary>
public class RefreshToken : IMustHaveTenant
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
}
