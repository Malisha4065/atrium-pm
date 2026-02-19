using AtriumPM.Shared.Interfaces;
using AtriumPM.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtriumPM.Billing.API.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BillingDbContext>
{
    public BillingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BillingDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=AtriumPM_Billing;User Id=sa;Password=Your_Strong_Password_123;TrustServerCertificate=True;");

        ITenantContext tenantContext = new TenantContext
        {
            TenantId = Guid.Empty
        };

        return new BillingDbContext(optionsBuilder.Options, tenantContext);
    }
}
