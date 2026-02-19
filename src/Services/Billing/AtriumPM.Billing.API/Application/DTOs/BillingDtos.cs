namespace AtriumPM.Billing.API.Application.DTOs;

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

public record PaymentDto(
    Guid Id,
    Guid InvoiceId,
    Guid TenantId,
    decimal Amount,
    string Method,
    DateTime PaidAt,
    DateTime CreatedAt);

public record CreatePaymentRequest(Guid InvoiceId, decimal Amount, string Method, DateTime PaidAt);
