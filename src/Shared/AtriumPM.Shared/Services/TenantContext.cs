using AtriumPM.Shared.Interfaces;

namespace AtriumPM.Shared.Services;

/// <summary>
/// Scoped implementation of ITenantContext.
/// Set once per request by TenantMiddleware and consumed
/// by BaseDbContext for automatic query filtering.
/// </summary>
public class TenantContext : ITenantContext
{
    private Guid _tenantId;

    public Guid TenantId
    {
        get => _tenantId;
        set
        {
            _tenantId = value;
            IsResolved = value != Guid.Empty;
        }
    }

    public bool IsResolved { get; private set; }
}
