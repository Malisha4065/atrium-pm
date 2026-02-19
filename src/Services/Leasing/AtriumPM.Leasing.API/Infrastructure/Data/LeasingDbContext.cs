using AtriumPM.Leasing.API.Domain.Entities;
using AtriumPM.Leasing.API.Domain.Enums;
using AtriumPM.Shared.Data;
using AtriumPM.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtriumPM.Leasing.API.Infrastructure.Data;

public class LeasingDbContext : BaseDbContext
{
    public LeasingDbContext(DbContextOptions<LeasingDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public DbSet<Lease> Leases => Set<Lease>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Lease>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.UnitId).IsRequired();
            entity.Property(l => l.StartDate).IsRequired();
            entity.Property(l => l.EndDate).IsRequired(false);
            entity.Property(l => l.ResidentName).IsRequired().HasMaxLength(200);
            entity.Property(l => l.Status)
                  .HasConversion<string>()
                  .HasMaxLength(30)
                  .HasDefaultValue(LeaseStatus.Draft);

            entity.HasIndex(l => new { l.TenantId, l.UnitId, l.Status });
            entity.HasIndex(l => new { l.TenantId, l.StartDate });
        });
    }
}
