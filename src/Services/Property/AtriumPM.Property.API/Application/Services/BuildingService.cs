using System.Text.Json;
using AtriumPM.Property.API.Application.DTOs;
using AtriumPM.Property.API.Application.Interfaces;
using AtriumPM.Property.API.Domain.Entities;
using AtriumPM.Property.API.Infrastructure.Data;
using AtriumPM.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace AtriumPM.Property.API.Application.Services;

public class BuildingService : IBuildingService
{
    private readonly PropertyDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IDistributedCache _cache;
    private readonly ILogger<BuildingService> _logger;

    public BuildingService(
        PropertyDbContext db,
        ITenantContext tenantContext,
        IDistributedCache cache,
        ILogger<BuildingService> logger)
    {
        _db = db;
        _tenantContext = tenantContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task<BuildingDto> CreateAsync(CreateBuildingRequest request)
    {
        EnsureTenantResolved();

        var entity = new Building
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            Address = request.Address.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Buildings.Add(entity);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created building {BuildingId} for tenant {TenantId}", entity.Id, entity.TenantId);

        return MapToDto(entity);
    }

    public async Task<IReadOnlyList<BuildingDto>> GetAllAsync()
    {
        return await _db.Buildings
            .AsNoTracking()
            .OrderBy(b => b.Address)
            .Select(b => new BuildingDto(b.Id, b.TenantId, b.Address, b.CreatedAt))
            .ToListAsync();
    }

    public async Task<BuildingDto?> GetByIdAsync(Guid id)
    {
        EnsureTenantResolved();

        var cacheKey = BuildCacheKey(id);
        var cached = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            var dto = JsonSerializer.Deserialize<BuildingDto>(cached);
            if (dto is not null)
            {
                return dto;
            }
        }

        var entity = await _db.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        if (entity is null)
        {
            return null;
        }

        var result = MapToDto(entity);

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        return result;
    }

    public async Task<BuildingDto?> UpdateAsync(Guid id, UpdateBuildingRequest request)
    {
        var entity = await _db.Buildings.FirstOrDefaultAsync(b => b.Id == id);
        if (entity is null)
        {
            return null;
        }

        entity.Address = request.Address.Trim();
        await _db.SaveChangesAsync();

        await _cache.RemoveAsync(BuildCacheKey(entity.Id));

        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _db.Buildings.FirstOrDefaultAsync(b => b.Id == id);
        if (entity is null)
        {
            return false;
        }

        _db.Buildings.Remove(entity);
        await _db.SaveChangesAsync();

        await _cache.RemoveAsync(BuildCacheKey(id));

        return true;
    }

    private void EnsureTenantResolved()
    {
        if (!_tenantContext.IsResolved)
        {
            throw new InvalidOperationException("Tenant context is not resolved for this request.");
        }
    }

    private string BuildCacheKey(Guid buildingId)
    {
        return $"property:building:{_tenantContext.TenantId}:{buildingId}";
    }

    private static BuildingDto MapToDto(Building entity)
    {
        return new BuildingDto(entity.Id, entity.TenantId, entity.Address, entity.CreatedAt);
    }
}
