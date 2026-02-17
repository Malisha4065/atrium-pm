namespace AtriumPM.Shared.Interfaces;

/// <summary>
/// Scoped service providing the current tenant context per-request.
/// Populated by TenantMiddleware from X-Tenant-ID header or JWT claims.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; set; }
    bool IsResolved { get; }
}
