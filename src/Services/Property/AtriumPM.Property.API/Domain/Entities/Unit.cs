using AtriumPM.Shared.Interfaces;

namespace AtriumPM.Property.API.Domain.Entities;

public class Unit : IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid BuildingId { get; set; }
    public Guid TenantId { get; set; }
    public int UnitNumber { get; set; }
    public bool IsOccupied { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Building Building { get; set; } = null!;
}
