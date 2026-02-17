using AtriumPM.Identity.API.Application.DTOs;
using AtriumPM.Identity.API.Application.Events;
using AtriumPM.Identity.API.Application.Interfaces;
using AtriumPM.Identity.API.Domain.Entities;
using AtriumPM.Identity.API.Domain.Enums;
using AtriumPM.Identity.API.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AtriumPM.Identity.API.Application.Services;

/// <summary>
/// Handles tenant registration (creates Tenant + admin User in a transaction)
/// and tenant lookups. Publishes TenantCreatedEvent via MassTransit.
/// </summary>
public class TenantService : ITenantService
{
    private readonly IdentityDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        IdentityDbContext db,
        IPublishEndpoint publishEndpoint,
        ILogger<TenantService> logger)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<TenantDto> RegisterTenantAsync(RegisterTenantRequest request)
    {
        // Check for duplicate subdomain
        var existing = await _db.Tenants
            .AnyAsync(t => t.SubDomain == request.SubDomain.ToLowerInvariant());

        if (existing)
            throw new InvalidOperationException($"SubDomain '{request.SubDomain}' is already taken.");

        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                SubDomain = request.SubDomain.ToLowerInvariant(),
                Status = TenantStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _db.Tenants.Add(tenant);
            await _db.SaveChangesAsync();

            // Create the admin user for this tenant
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = request.AdminEmail.ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword),
                FirstName = request.AdminFirstName,
                LastName = request.AdminLastName,
                Role = UserRole.TenantAdmin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(adminUser);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("Tenant {TenantName} ({TenantId}) registered with admin {AdminEmail}",
                tenant.Name, tenant.Id, adminUser.Email);

            // Publish event for other services
            await _publishEndpoint.Publish(new TenantCreatedEvent(
                tenant.Id, tenant.Name, tenant.SubDomain, tenant.CreatedAt));

            return new TenantDto(tenant.Id, tenant.Name, tenant.SubDomain,
                tenant.Status.ToString(), tenant.CreatedAt);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TenantDto?> GetTenantByIdAsync(Guid id)
    {
        var tenant = await _db.Tenants.FindAsync(id);
        if (tenant is null) return null;

        return new TenantDto(tenant.Id, tenant.Name, tenant.SubDomain,
            tenant.Status.ToString(), tenant.CreatedAt);
    }

    public async Task<IEnumerable<TenantDto>> GetAllTenantsAsync()
    {
        return await _db.Tenants
            .AsNoTracking()
            .Select(t => new TenantDto(t.Id, t.Name, t.SubDomain,
                t.Status.ToString(), t.CreatedAt))
            .ToListAsync();
    }
}
