namespace AtriumPM.Property.API.Application.DTOs;

public record BuildingDto(Guid Id, Guid TenantId, string Address, DateTime CreatedAt);

public record CreateBuildingRequest(string Address);

public record UpdateBuildingRequest(string Address);
