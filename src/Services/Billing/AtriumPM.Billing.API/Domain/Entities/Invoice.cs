using AtriumPM.Billing.API.Domain.Enums;
using AtriumPM.Shared.Interfaces;

namespace AtriumPM.Billing.API.Domain.Entities;

public class Invoice : IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid LeaseId { get; set; }
    public Guid UnitId { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<LateFeeCharge> LateFees { get; set; } = new List<LateFeeCharge>();
}
