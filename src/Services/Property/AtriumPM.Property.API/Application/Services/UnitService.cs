using AtriumPM.Property.API.Application.DTOs;
using AtriumPM.Property.API.Application.Interfaces;
using AtriumPM.Property.API.Domain.Entities;
using AtriumPM.Property.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AtriumPM.Property.API.Application.Services;

public class UnitService : IUnitService
{
    private readonly PropertyDbContext _db;

    public UnitService(PropertyDbContext db)
    {
        _db = db;
    }

    public async Task<UnitDto> CreateAsync(CreateUnitRequest request)
    {
        var building = await _db.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == request.BuildingId);
        if (building is null)
        {
            throw new InvalidOperationException("Building not found for current tenant.");
        }

        var unit = new Unit
        {
            Id = Guid.NewGuid(),
            BuildingId = request.BuildingId,
            TenantId = building.TenantId,
            UnitNumber = request.UnitNumber,
            IsOccupied = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Units.Add(unit);
        await _db.SaveChangesAsync();

        return MapToDto(unit);
    }

    public async Task<IReadOnlyList<UnitDto>> GetAllAsync()
    {
        return await _db.Units
            .AsNoTracking()
            .OrderBy(u => u.UnitNumber)
            .Select(u => new UnitDto(u.Id, u.BuildingId, u.TenantId, u.UnitNumber, u.IsOccupied, u.CreatedAt))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<UnitDto>> GetByBuildingIdAsync(Guid buildingId)
    {
        return await _db.Units
            .AsNoTracking()
            .Where(u => u.BuildingId == buildingId)
            .OrderBy(u => u.UnitNumber)
            .Select(u => new UnitDto(u.Id, u.BuildingId, u.TenantId, u.UnitNumber, u.IsOccupied, u.CreatedAt))
            .ToListAsync();
    }

    public async Task<UnitDto?> GetByIdAsync(Guid id)
    {
        var unit = await _db.Units.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return unit is null ? null : MapToDto(unit);
    }

    public async Task<UnitDto?> UpdateAsync(Guid id, UpdateUnitRequest request)
    {
        var unit = await _db.Units.FirstOrDefaultAsync(u => u.Id == id);
        if (unit is null)
        {
            return null;
        }

        unit.UnitNumber = request.UnitNumber;
        unit.IsOccupied = request.IsOccupied;
        await _db.SaveChangesAsync();

        return MapToDto(unit);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var unit = await _db.Units.FirstOrDefaultAsync(u => u.Id == id);
        if (unit is null)
        {
            return false;
        }

        _db.Units.Remove(unit);
        await _db.SaveChangesAsync();
        return true;
    }

    private static UnitDto MapToDto(Unit unit)
    {
        return new UnitDto(unit.Id, unit.BuildingId, unit.TenantId, unit.UnitNumber, unit.IsOccupied, unit.CreatedAt);
    }
}
