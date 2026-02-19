using AtriumPM.Property.API.Domain.Entities;
using AtriumPM.Shared.Data;
using AtriumPM.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtriumPM.Property.API.Infrastructure.Data;

public class PropertyDbContext : BaseDbContext
{
    public PropertyDbContext(DbContextOptions<PropertyDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Unit> Units => Set<Unit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Address).IsRequired().HasMaxLength(300);
            entity.HasIndex(b => new { b.TenantId, b.Address });
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.UnitNumber).IsRequired();
            entity.Property(u => u.IsOccupied).IsRequired();

            entity.HasIndex(u => new { u.TenantId, u.BuildingId, u.UnitNumber })
                  .IsUnique();

            entity.HasOne(u => u.Building)
                  .WithMany(b => b.Units)
                  .HasForeignKey(u => u.BuildingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
