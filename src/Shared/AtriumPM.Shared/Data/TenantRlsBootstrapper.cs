using AtriumPM.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtriumPM.Shared.Data;

public static class TenantRlsBootstrapper
{
    public static async Task EnsureTenantRlsPoliciesAsync(this DbContext dbContext, CancellationToken cancellationToken = default)
    {
        var tenantTables = dbContext.Model.GetEntityTypes()
            .Where(entityType => typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
            .Select(entityType => new
            {
                Schema = entityType.GetSchema() ?? "dbo",
                Table = entityType.GetTableName()
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Table))
            .Distinct()
            .ToList();

        if (tenantTables.Count == 0)
            return;

        const string functionSql = """
            IF OBJECT_ID(N'[dbo].[fn_atriumpm_tenantpredicate]', N'IF') IS NULL
            EXEC(N'CREATE FUNCTION [dbo].[fn_atriumpm_tenantpredicate](@TenantId uniqueidentifier)
                  RETURNS TABLE
                  WITH SCHEMABINDING
                  AS
                  RETURN SELECT 1 AS [fn_result]
                WHERE CAST(SESSION_CONTEXT(N''TenantId'') AS uniqueidentifier) = @TenantId;');
            """;

        await dbContext.Database.ExecuteSqlRawAsync(functionSql, cancellationToken);

        foreach (var table in tenantTables)
        {
            var schema = EscapeIdentifier(table.Schema);
            var tableName = EscapeIdentifier(table.Table!);
            var objectName = $"[{schema}].[{tableName}]";
            var policyName = EscapeIdentifier($"AtriumPMTenantPolicy_{schema}_{tableName}");

            var sql = $"""
                IF COL_LENGTH(N'{objectName}', N'TenantId') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.security_policies WHERE [name] = N'{policyName}' AND [schema_id] = SCHEMA_ID(N'dbo'))
                    BEGIN
                        EXEC(N'CREATE SECURITY POLICY [dbo].[{policyName}]
                            ADD FILTER PREDICATE [dbo].[fn_atriumpm_tenantpredicate]([TenantId]) ON {objectName},
                            ADD BLOCK PREDICATE [dbo].[fn_atriumpm_tenantpredicate]([TenantId]) ON {objectName} AFTER INSERT,
                            ADD BLOCK PREDICATE [dbo].[fn_atriumpm_tenantpredicate]([TenantId]) ON {objectName} BEFORE UPDATE
                            WITH (STATE = ON)');
                    END
                END
                """;

            await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }
    }

    private static string EscapeIdentifier(string identifier)
    {
        return identifier.Replace("]", "]]", StringComparison.Ordinal);
    }
}
