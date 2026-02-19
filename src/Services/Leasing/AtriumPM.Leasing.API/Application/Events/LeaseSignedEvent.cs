namespace AtriumPM.Leasing.API.Application.Events;

public record LeaseSignedEvent(
    Guid LeaseId,
    Guid TenantId,
    Guid UnitId,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime SignedAtUtc);
