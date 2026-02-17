using System.Linq.Expressions;
using AtriumPM.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtriumPM.Shared.Data;

/// <summary>
/// Base EF Core DbContext that enforces multi-tenancy via:
/// 1. Global Query Filters — automatically filter every IMustHaveTenant entity by TenantId.
/// 2. Shadow Properties — stamps TenantId, CreatedDate, ModifiedDate on SaveChanges.
/// 3. Row-Level Security — sets SQL Server SESSION_CONTEXT for Dapper/raw SQL safety.
/// </summary>
public abstract class BaseDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    protected BaseDbContext(DbContextOptions options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
                continue;

            // Ensure TenantId exists in the EF model. If a CLR property exists,
            // this maps it; otherwise it is created as a shadow property.
            modelBuilder.Entity(entityType.ClrType)
                .Property<Guid>("TenantId")
                .IsRequired();

            // Add shadow properties for audit trail
            modelBuilder.Entity(entityType.ClrType)
                .Property<DateTime>("CreatedDate")
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity(entityType.ClrType)
                .Property<DateTime>("ModifiedDate")
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity(entityType.ClrType)
                .Property<string>("ModifiedBy")
                .HasMaxLength(256)
                .IsRequired(false);

            // Apply global query filter: WHERE EF.Property<Guid>(entity, "TenantId") == currentTenantId
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var tenantIdProperty = Expression.Call(
                typeof(EF),
                nameof(EF.Property),
                new[] { typeof(Guid) },
                parameter,
                Expression.Constant("TenantId"));
            var tenantIdValue = Expression.Property(Expression.Constant(_tenantContext), nameof(ITenantContext.TenantId));
            var filter = Expression.Lambda(Expression.Equal(tenantIdProperty, tenantIdValue), parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampTenantAndAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        StampTenantAndAuditFields();
        return base.SaveChanges();
    }

    private void StampTenantAndAuditFields()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not IMustHaveTenant)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    // Stamp tenant ID if not already set
                    if (_tenantContext.IsResolved)
                    {
                        var tenantProperty = entry.Property("TenantId");
                        if (tenantProperty.CurrentValue is null || (Guid)tenantProperty.CurrentValue == Guid.Empty)
                            tenantProperty.CurrentValue = _tenantContext.TenantId;
                    }

                    entry.Property("CreatedDate").CurrentValue = now;
                    entry.Property("ModifiedDate").CurrentValue = now;
                    break;

                case EntityState.Modified:
                    entry.Property("ModifiedDate").CurrentValue = now;
                    // Prevent TenantId from being changed
                    entry.Property("TenantId").IsModified = false;
                    break;
            }
        }
    }

    /// <summary>
    /// Sets SQL Server SESSION_CONTEXT for Row-Level Security.
    /// Call this before executing raw SQL or Dapper queries.
    /// </summary>
    public async Task SetSessionContextAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.IsResolved) return;

        await Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sp_set_session_context @key=N'TenantId', @value={_tenantContext.TenantId.ToString()}",
            cancellationToken);
    }
}
