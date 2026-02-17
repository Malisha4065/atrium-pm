using AtriumPM.Identity.API.Domain.Enums;

namespace AtriumPM.Identity.API.Domain.Entities;

/// <summary>
/// Root aggregate representing a Property Management Company.
/// Does NOT implement IMustHaveTenant — the Tenant IS the tenant.
/// </summary>
public class Tenant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SubDomain { get; set; } = string.Empty;

    public TenantStatus Status { get; set; } = TenantStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional per-tenant connection string for future database-per-tenant isolation.
    /// </summary>
    public string? ConnectionString { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}
