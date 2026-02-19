using AtriumPM.Property.API.Application.DTOs;

namespace AtriumPM.Property.API.Application.Interfaces;

public interface IUnitService
{
    Task<UnitDto> CreateAsync(CreateUnitRequest request);
    Task<IReadOnlyList<UnitDto>> GetAllAsync();
    Task<IReadOnlyList<UnitDto>> GetByBuildingIdAsync(Guid buildingId);
    Task<UnitDto?> GetByIdAsync(Guid id);
    Task<UnitDto?> UpdateAsync(Guid id, UpdateUnitRequest request);
    Task<bool> DeleteAsync(Guid id);
}
