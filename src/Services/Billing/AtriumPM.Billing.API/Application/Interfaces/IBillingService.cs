using AtriumPM.Billing.API.Application.DTOs;

namespace AtriumPM.Billing.API.Application.Interfaces;

public interface IBillingService
{
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request);
    Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync();
    Task<InvoiceDto?> GetInvoiceByIdAsync(Guid id);
    Task<PaymentDto> RecordPaymentAsync(CreatePaymentRequest request);
}
