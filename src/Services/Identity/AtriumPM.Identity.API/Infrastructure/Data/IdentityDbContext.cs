using AtriumPM.Identity.API.Domain.Entities;
using AtriumPM.Identity.API.Domain.Enums;
using AtriumPM.Shared.Data;
using AtriumPM.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtriumPM.Identity.API.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for the Identity & Tenant service.
/// Extends BaseDbContext to inherit multi-tenant query filters.
/// </summary>
public class IdentityDbContext : BaseDbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Tenant ──────────────────────────────────
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.SubDomain).IsRequired().HasMaxLength(100);
            entity.HasIndex(t => t.SubDomain).IsUnique();
            entity.Property(t => t.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(t => t.ConnectionString).HasMaxLength(500);
        });

        // ── User ────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Role)
                  .HasConversion<string>()
                  .HasMaxLength(30);

            entity.HasOne(u => u.Tenant)
                  .WithMany(t => t.Users)
                  .HasForeignKey(u => u.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── RefreshToken ────────────────────────────
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(512);
            entity.HasIndex(rt => rt.Token).IsUnique();

            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
