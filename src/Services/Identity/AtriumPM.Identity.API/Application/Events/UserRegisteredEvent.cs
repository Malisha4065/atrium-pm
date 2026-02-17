namespace AtriumPM.Identity.API.Application.Events;

/// <summary>
/// Published via MassTransit when a new user is registered under a tenant.
/// </summary>
public record UserRegisteredEvent(
    Guid UserId,
    Guid TenantId,
    string Email,
    string Role,
    DateTime CreatedAt
);
