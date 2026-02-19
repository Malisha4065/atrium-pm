using AtriumPM.Shared.Interfaces;
using AtriumPM.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtriumPM.Property.API.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PropertyDbContext>
{
    public PropertyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PropertyDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=AtriumPM_Property;User Id=sa;Password=Your_Strong_Password_123;TrustServerCertificate=True;");

        ITenantContext tenantContext = new TenantContext
        {
            TenantId = Guid.Empty
        };

        return new PropertyDbContext(optionsBuilder.Options, tenantContext);
    }
}
