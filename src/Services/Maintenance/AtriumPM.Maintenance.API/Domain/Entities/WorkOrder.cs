using AtriumPM.Maintenance.API.Domain.Enums;
using AtriumPM.Shared.Interfaces;

namespace AtriumPM.Maintenance.API.Domain.Entities;

public class WorkOrder : IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid TenantId { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public MaintenanceTicket Ticket { get; set; } = null!;
}
