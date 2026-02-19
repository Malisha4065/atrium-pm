using AtriumPM.Shared.Interfaces;
using AtriumPM.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtriumPM.Maintenance.API.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MaintenanceDbContext>
{
    public MaintenanceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MaintenanceDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=AtriumPM_Maintenance;User Id=sa;Password=Your_Strong_Password_123;TrustServerCertificate=True;");

        ITenantContext tenantContext = new TenantContext
        {
            TenantId = Guid.Empty
        };

        return new MaintenanceDbContext(optionsBuilder.Options, tenantContext);
    }
}
