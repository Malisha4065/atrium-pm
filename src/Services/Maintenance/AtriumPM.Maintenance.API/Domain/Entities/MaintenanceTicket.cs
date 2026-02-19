using AtriumPM.Maintenance.API.Domain.Enums;
using AtriumPM.Shared.Interfaces;

namespace AtriumPM.Maintenance.API.Domain.Entities;

public class MaintenanceTicket : IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid? UnitId { get; set; }
    public Guid? BuildingId { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
