using System.Security.Claims;
using AtriumPM.Shared.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AtriumPM.Shared.Middleware;

/// <summary>
/// Middleware that resolves the current tenant from request headers or JWT claims.
/// Priority: X-Tenant-ID header > tenant_id JWT claim.
/// Certain paths (registration, login, health, swagger) are exempt.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly HashSet<string> ExemptPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/tenants/register",
        "/api/auth/login",
        "/health",
        "/swagger"
    };

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip tenant resolution for exempt paths
        if (IsExemptPath(path))
        {
            await _next(context);
            return;
        }

        // 1. Try X-Tenant-ID header
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var headerTenantId)
            && Guid.TryParse(headerTenantId.ToString(), out var tenantIdFromHeader))
        {
            tenantContext.TenantId = tenantIdFromHeader;
            await _next(context);
            return;
        }

        // 2. Fall back to tenant_id claim from JWT
        var tenantClaim = context.User.FindFirst("tenant_id")
                          ?? context.User.FindFirst(ClaimTypes.GroupSid);

        if (tenantClaim is not null && Guid.TryParse(tenantClaim.Value, out var tenantIdFromClaim))
        {
            tenantContext.TenantId = tenantIdFromClaim;
            await _next(context);
            return;
        }

        // No tenant resolved — return 400
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Tenant context could not be resolved. Provide X-Tenant-ID header or authenticate with a valid JWT."
        });
    }

    private static bool IsExemptPath(string path)
    {
        foreach (var exempt in ExemptPaths)
        {
            if (path.StartsWith(exempt, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
