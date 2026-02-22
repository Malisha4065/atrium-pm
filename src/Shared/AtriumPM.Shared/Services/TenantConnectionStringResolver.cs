using AtriumPM.Shared.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AtriumPM.Shared.Services;

public class TenantConnectionStringResolver : ITenantConnectionStringResolver
{
    private readonly ITenantContext _tenantContext;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<TenantConnectionStringResolver> _logger;

    public TenantConnectionStringResolver(
        ITenantContext tenantContext,
        IConfiguration configuration,
        IMemoryCache memoryCache,
        ILogger<TenantConnectionStringResolver> logger)
    {
        _tenantContext = tenantContext;
        _configuration = configuration;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public string ResolveConnectionString(string defaultConnectionString)
    {
        if (!_tenantContext.IsResolved)
            return defaultConnectionString;

        var tenantConnectionTemplate = GetTenantConnectionTemplate(_tenantContext.TenantId);
        if (string.IsNullOrWhiteSpace(tenantConnectionTemplate))
            return defaultConnectionString;

        try
        {
            var defaultBuilder = new SqlConnectionStringBuilder(defaultConnectionString);
            var defaultDatabase = defaultBuilder.InitialCatalog;

            if (string.IsNullOrWhiteSpace(defaultDatabase))
                return defaultConnectionString;

            if (tenantConnectionTemplate.Contains("{db}", StringComparison.OrdinalIgnoreCase))
            {
                return tenantConnectionTemplate.Replace("{db}", defaultDatabase, StringComparison.OrdinalIgnoreCase);
            }

            var tenantBuilder = new SqlConnectionStringBuilder(tenantConnectionTemplate);
            if (string.IsNullOrWhiteSpace(tenantBuilder.InitialCatalog)
                || tenantBuilder.InitialCatalog.Equals("AtriumPM_Identity", StringComparison.OrdinalIgnoreCase))
            {
                tenantBuilder.InitialCatalog = defaultDatabase;
            }

            return tenantBuilder.ConnectionString;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to resolve tenant-specific connection string for tenant {TenantId}. Falling back to default.",
                _tenantContext.TenantId);
            return defaultConnectionString;
        }
    }

    private string? GetTenantConnectionTemplate(Guid tenantId)
    {
        var cacheKey = $"tenant-conn-template:{tenantId}";
        if (_memoryCache.TryGetValue(cacheKey, out string? cached))
            return cached;

        var identityConnection = _configuration.GetConnectionString("IdentityDb");
        if (string.IsNullOrWhiteSpace(identityConnection))
            return null;

        try
        {
            using var connection = new SqlConnection(identityConnection);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT TOP 1 [ConnectionString] FROM [Tenants] WHERE [Id] = @tenantId";
            command.Parameters.AddWithValue("@tenantId", tenantId);

            var result = command.ExecuteScalar() as string;
            if (string.IsNullOrWhiteSpace(result))
                return null;

            _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed reading tenant connection mapping from identity DB for tenant {TenantId}.", tenantId);
            return null;
        }
    }
}
