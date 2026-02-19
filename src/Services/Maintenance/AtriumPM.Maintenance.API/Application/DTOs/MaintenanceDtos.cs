namespace AtriumPM.Maintenance.API.Application.DTOs;

public record MaintenanceTicketDto(
    Guid Id,
    Guid? UnitId,
    Guid? BuildingId,
    Guid TenantId,
    string Title,
    string Description,
    string Priority,
    string Status,
    DateTime CreatedAt);

public record CreateMaintenanceTicketRequest(
    Guid? UnitId,
    Guid? BuildingId,
    string Title,
    string Description,
    string Priority);

public record UpdateMaintenanceTicketStatusRequest(string Status);

public record WorkOrderDto(
    Guid Id,
    Guid TicketId,
    Guid TenantId,
    string AssignedTo,
    DateTime ScheduledAt,
    string Status,
    DateTime CreatedAt);

public record CreateWorkOrderRequest(Guid TicketId, string AssignedTo, DateTime ScheduledAt);

public record UpdateWorkOrderStatusRequest(string Status);
