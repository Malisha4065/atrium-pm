using AtriumPM.Shared.Interfaces;
using AtriumPM.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtriumPM.Identity.API.Infrastructure.Data;

/// <summary>
/// Design-time factory for EF Core migrations tooling.
/// Provides a dummy TenantContext so migrations can run without HTTP context.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=AtriumPM_Identity;User Id=sa;Password=Your_Strong_Password_123;TrustServerCertificate=True;");

        // Provide a no-op tenant context for migration generation
        var tenantContext = new TenantContext();
        return new IdentityDbContext(optionsBuilder.Options, tenantContext);
    }
}
