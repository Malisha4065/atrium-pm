using AtriumPM.Shared.Interfaces;

namespace AtriumPM.Billing.API.Domain.Entities;

public class LateFeeCharge : IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }

    public Invoice Invoice { get; set; } = null!;
}
