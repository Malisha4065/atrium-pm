using System.Data.Common;
using AtriumPM.Shared.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AtriumPM.Shared.Data;

public class TenantSessionContextConnectionInterceptor : DbConnectionInterceptor
{
    private readonly ITenantContext _tenantContext;

    public TenantSessionContextConnectionInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        SetTenantSessionContext(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await SetTenantSessionContextAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private void SetTenantSessionContext(DbConnection connection)
    {
        if (!_tenantContext.IsResolved)
            return;

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_set_session_context @key=N'TenantId', @value=@TenantId";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@TenantId";
        parameter.Value = _tenantContext.TenantId.ToString();
        command.Parameters.Add(parameter);

        command.ExecuteNonQuery();
    }

    private async Task SetTenantSessionContextAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return;

        await using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_set_session_context @key=N'TenantId', @value=@TenantId";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@TenantId";
        parameter.Value = _tenantContext.TenantId.ToString();
        command.Parameters.Add(parameter);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
