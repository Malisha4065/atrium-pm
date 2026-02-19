using AtriumPM.Property.API.Application.DTOs;

namespace AtriumPM.Property.API.Application.Interfaces;

public interface IBuildingService
{
    Task<BuildingDto> CreateAsync(CreateBuildingRequest request);
    Task<IReadOnlyList<BuildingDto>> GetAllAsync();
    Task<BuildingDto?> GetByIdAsync(Guid id);
    Task<BuildingDto?> UpdateAsync(Guid id, UpdateBuildingRequest request);
    Task<bool> DeleteAsync(Guid id);
}
