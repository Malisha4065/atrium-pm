using AtriumPM.Leasing.API.Domain.Enums;
using AtriumPM.Shared.Interfaces;

namespace AtriumPM.Leasing.API.Domain.Entities;

public class Lease : IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public Guid TenantId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public LeaseStatus Status { get; set; } = LeaseStatus.Draft;
    public string ResidentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
