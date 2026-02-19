namespace AtriumPM.Property.API.Application.DTOs;

public record UnitDto(Guid Id, Guid BuildingId, Guid TenantId, int UnitNumber, bool IsOccupied, DateTime CreatedAt);

public record CreateUnitRequest(Guid BuildingId, int UnitNumber);

public record UpdateUnitRequest(int UnitNumber, bool IsOccupied);
