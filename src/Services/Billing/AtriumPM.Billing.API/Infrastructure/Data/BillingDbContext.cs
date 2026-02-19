using AtriumPM.Billing.API.Domain.Entities;
using AtriumPM.Billing.API.Domain.Enums;
using AtriumPM.Shared.Data;
using AtriumPM.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtriumPM.Billing.API.Infrastructure.Data;

public class BillingDbContext : BaseDbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<LateFeeCharge> LateFeeCharges => Set<LateFeeCharge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.BaseAmount).HasColumnType("decimal(18,2)");
            entity.Property(i => i.LateFeeAmount).HasColumnType("decimal(18,2)");
            entity.Property(i => i.PaidAmount).HasColumnType("decimal(18,2)");
            entity.Property(i => i.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(InvoiceStatus.Open);
            entity.HasIndex(i => new { i.TenantId, i.LeaseId, i.DueDate });
            entity.HasIndex(i => new { i.TenantId, i.Status, i.DueDate });
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Method)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(PaymentMethod.Unknown);

            entity.HasOne(p => p.Invoice)
                  .WithMany(i => i.Payments)
                  .HasForeignKey(p => p.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => new { p.TenantId, p.InvoiceId, p.PaidAt });
        });

        modelBuilder.Entity<LateFeeCharge>(entity =>
        {
            entity.HasKey(lf => lf.Id);
            entity.Property(lf => lf.Amount).HasColumnType("decimal(18,2)");
            entity.Property(lf => lf.Reason).HasMaxLength(500).IsRequired();

            entity.HasOne(lf => lf.Invoice)
                  .WithMany(i => i.LateFees)
                  .HasForeignKey(lf => lf.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(lf => new { lf.TenantId, lf.InvoiceId, lf.AppliedAt });
        });
    }
}
