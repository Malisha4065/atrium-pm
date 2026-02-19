using AtriumPM.Shared.Interfaces;
using AtriumPM.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtriumPM.Leasing.API.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LeasingDbContext>
{
    public LeasingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LeasingDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=AtriumPM_Leasing;User Id=sa;Password=Your_Strong_Password_123;TrustServerCertificate=True;");

        ITenantContext tenantContext = new TenantContext
        {
            TenantId = Guid.Empty
        };

        return new LeasingDbContext(optionsBuilder.Options, tenantContext);
    }
}
