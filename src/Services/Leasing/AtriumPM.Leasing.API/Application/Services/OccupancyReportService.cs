using System.Data;
using AtriumPM.Leasing.API.Application.DTOs;
using AtriumPM.Leasing.API.Application.Interfaces;
using AtriumPM.Shared.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AtriumPM.Leasing.API.Application.Services;

public class OccupancyReportService : IOccupancyReportService
{
    private readonly string _connectionString;
    private readonly ITenantContext _tenantContext;

    public OccupancyReportService(IConfiguration configuration, ITenantContext tenantContext)
    {
        _connectionString = configuration.GetConnectionString("LeasingDb")
            ?? throw new InvalidOperationException("Connection string 'LeasingDb' is not configured.");
        _tenantContext = tenantContext;
    }

    public async Task<OccupancySummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await SetSessionContextAsync(connection);

        const string sql = """
            SELECT
                COUNT(1) AS TotalLeases,
                SUM(CASE WHEN [Status] = 'Active' THEN 1 ELSE 0 END) AS ActiveLeases,
                SUM(CASE WHEN [Status] = 'Draft' THEN 1 ELSE 0 END) AS DraftLeases,
                SUM(CASE WHEN [Status] = 'Expired' THEN 1 ELSE 0 END) AS ExpiredLeases,
                CAST(
                    CASE WHEN COUNT(1) = 0 THEN 0
                         ELSE (100.0 * SUM(CASE WHEN [Status] = 'Active' THEN 1 ELSE 0 END)) / COUNT(1)
                    END AS decimal(10,2)
                ) AS OccupancyRate
            FROM [Leases]
            WHERE [TenantId] = CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier);
            """;

        var result = await connection.QuerySingleAsync<OccupancySummaryDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return result;
    }

    public async Task<IReadOnlyList<UnitOccupancyDto>> GetUnitOccupancyAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await SetSessionContextAsync(connection);

        const string sql = """
            SELECT
                [UnitId],
                SUM(CASE WHEN [Status] = 'Active' THEN 1 ELSE 0 END) AS ActiveLeaseCount,
                MAX([StartDate]) AS LastLeaseStartDate
            FROM [Leases]
            WHERE [TenantId] = CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier)
            GROUP BY [UnitId]
            ORDER BY [UnitId];
            """;

        var result = await connection.QueryAsync<UnitOccupancyDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return result.ToList();
    }

    private async Task SetSessionContextAsync(IDbConnection connection)
    {
        if (!_tenantContext.IsResolved)
        {
            throw new InvalidOperationException("Tenant context is not resolved for this request.");
        }

        const string sql = "EXEC sp_set_session_context @key=N'TenantId', @value=@TenantId";
        await connection.ExecuteAsync(sql, new { TenantId = _tenantContext.TenantId.ToString() });
    }
}
