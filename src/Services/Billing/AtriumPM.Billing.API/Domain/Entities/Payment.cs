using AtriumPM.Billing.API.Domain.Enums;
using AtriumPM.Shared.Interfaces;

namespace AtriumPM.Billing.API.Domain.Entities;

public class Payment : IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid TenantId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.Unknown;
    public DateTime PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Invoice Invoice { get; set; } = null!;
}
