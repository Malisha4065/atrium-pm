namespace AtriumPM.Identity.API.Application.Events;

/// <summary>
/// Published via MassTransit when a new tenant registers.
/// Other services can consume this to provision tenant-specific resources.
/// </summary>
public record TenantCreatedEvent(
    Guid TenantId,
    string Name,
    string SubDomain,
    DateTime CreatedAt
);
