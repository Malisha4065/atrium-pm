namespace AtriumPM.Web.Models;

public record RegisterTenantRequest(
    string Name,
    string SubDomain,
    string AdminEmail,
    string AdminPassword,
    string AdminFirstName,
    string AdminLastName);

public record TenantDto(Guid Id, string Name, string SubDomain, string Status, DateTime CreatedAt);

public record LoginRequest(string Email, string Password, Guid TenantId);

public record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public record BuildingDto(Guid Id, Guid TenantId, string Address, DateTime CreatedAt);
public record CreateBuildingRequest(string Address);

public record UnitDto(Guid Id, Guid BuildingId, Guid TenantId, int UnitNumber, bool IsOccupied, DateTime CreatedAt);
public record CreateUnitRequest(Guid BuildingId, int UnitNumber);

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
public record CreateMaintenanceTicketRequest(Guid? UnitId, Guid? BuildingId, string Title, string Description, string Priority);

public record InvoiceDto(
    Guid Id,
    Guid TenantId,
    Guid LeaseId,
    Guid UnitId,
    decimal BaseAmount,
    decimal LateFeeAmount,
    decimal PaidAmount,
    DateTime DueDate,
    string Status,
    DateTime CreatedAt);
public record CreateInvoiceRequest(Guid LeaseId, Guid UnitId, decimal BaseAmount, DateTime DueDate);

public record PaymentDto(Guid Id, Guid InvoiceId, Guid TenantId, decimal Amount, string Method, DateTime PaidAt, DateTime CreatedAt);
public record CreatePaymentRequest(Guid InvoiceId, decimal Amount, string Method, DateTime PaidAt);
