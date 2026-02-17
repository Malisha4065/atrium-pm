namespace AtriumPM.Shared.Interfaces;

/// <summary>
/// Marker interface for entities that belong to a specific tenant.
/// All entities implementing this interface will have automatic
/// TenantId filtering applied via EF Core Global Query Filters.
/// </summary>
public interface IMustHaveTenant
{
    Guid TenantId { get; set; }
}
