namespace AtriumPM.Leasing.API.Application.DTOs;

public record LeaseDto(
    Guid Id,
    Guid UnitId,
    Guid TenantId,
    DateTime StartDate,
    DateTime? EndDate,
    string Status,
    string ResidentName,
    DateTime CreatedAt);

public record CreateLeaseRequest(Guid UnitId, DateTime StartDate, DateTime? EndDate, string ResidentName);

public record UpdateLeaseStatusRequest(string Status);
