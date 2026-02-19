using AtriumPM.Maintenance.API.Domain.Entities;
using AtriumPM.Maintenance.API.Domain.Enums;
using AtriumPM.Shared.Data;
using AtriumPM.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtriumPM.Maintenance.API.Infrastructure.Data;

public class MaintenanceDbContext : BaseDbContext
{
    public MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public DbSet<MaintenanceTicket> Tickets => Set<MaintenanceTicket>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MaintenanceTicket>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(2000);
            entity.Property(t => t.Priority)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(TicketPriority.Medium);
            entity.Property(t => t.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(TicketStatus.Open);

            entity.ToTable(tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_MaintenanceTicket_Target",
                    "([UnitId] IS NOT NULL AND [BuildingId] IS NULL) OR ([UnitId] IS NULL AND [BuildingId] IS NOT NULL)");
            });

            entity.HasIndex(t => new { t.TenantId, t.Status, t.Priority });
            entity.HasIndex(t => new { t.TenantId, t.UnitId });
            entity.HasIndex(t => new { t.TenantId, t.BuildingId });
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.AssignedTo).IsRequired().HasMaxLength(200);
            entity.Property(w => w.ScheduledAt).IsRequired();
            entity.Property(w => w.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(WorkOrderStatus.Pending);

            entity.HasOne(w => w.Ticket)
                  .WithMany(t => t.WorkOrders)
                  .HasForeignKey(w => w.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(w => new { w.TenantId, w.Status, w.ScheduledAt });
        });
    }
}
