using AtriumPM.Identity.API.Domain.Enums;
using AtriumPM.Shared.Interfaces;

namespace AtriumPM.Identity.API.Domain.Entities;

/// <summary>
/// Represents an authenticated user belonging to a tenant.
/// Implements IMustHaveTenant to enable automatic RLS filtering.
/// </summary>
public class User : IMustHaveTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Resident;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
