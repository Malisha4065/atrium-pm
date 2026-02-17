using System.ComponentModel.DataAnnotations;

namespace AtriumPM.Identity.API.Application.DTOs;

// ── Tenant DTOs ──────────────────────────────────

public record RegisterTenantRequest(
    [Required] [StringLength(200)] string Name,
    [Required] [StringLength(100)] string SubDomain,
    [Required] [EmailAddress] string AdminEmail,
    [Required] [MinLength(8)] string AdminPassword,
    [Required] [StringLength(100)] string AdminFirstName,
    [Required] [StringLength(100)] string AdminLastName
);

public record TenantDto(
    Guid Id,
    string Name,
    string SubDomain,
    string Status,
    DateTime CreatedAt
);

// ── Auth DTOs ────────────────────────────────────

public record LoginRequest(
    [Required] [EmailAddress] string Email,
    [Required] string Password,
    [Required] Guid TenantId
);

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);

public record RefreshTokenRequest(
    [Required] string RefreshToken,
    [Required] Guid TenantId
);

// ── User DTOs ────────────────────────────────────

public record RegisterUserRequest(
    [Required] [EmailAddress] string Email,
    [Required] [MinLength(8)] string Password,
    [Required] [StringLength(100)] string FirstName,
    [Required] [StringLength(100)] string LastName,
    [Required] string Role
);

public record UserDto(
    Guid Id,
    Guid TenantId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);
