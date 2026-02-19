using AtriumPM.Maintenance.API.Application.DTOs;
using AtriumPM.Maintenance.API.Application.Interfaces;
using AtriumPM.Maintenance.API.Domain.Entities;
using AtriumPM.Maintenance.API.Domain.Enums;
using AtriumPM.Maintenance.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AtriumPM.Maintenance.API.Application.Services;

public class WorkOrderService : IWorkOrderService
{
    private readonly MaintenanceDbContext _db;

    public WorkOrderService(MaintenanceDbContext db)
    {
        _db = db;
    }

    public async Task<WorkOrderDto> CreateAsync(CreateWorkOrderRequest request)
    {
        var ticket = await _db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == request.TicketId);
        if (ticket is null)
        {
            throw new InvalidOperationException("Ticket not found for current tenant.");
        }

        var workOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            TicketId = request.TicketId,
            TenantId = ticket.TenantId,
            AssignedTo = request.AssignedTo.Trim(),
            ScheduledAt = request.ScheduledAt,
            Status = WorkOrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.WorkOrders.Add(workOrder);
        await _db.SaveChangesAsync();

        return MapToDto(workOrder);
    }

    public async Task<IReadOnlyList<WorkOrderDto>> GetAllAsync()
    {
        return await _db.WorkOrders
            .AsNoTracking()
            .OrderBy(w => w.ScheduledAt)
            .Select(w => new WorkOrderDto(
                w.Id,
                w.TicketId,
                w.TenantId,
                w.AssignedTo,
                w.ScheduledAt,
                w.Status.ToString(),
                w.CreatedAt))
            .ToListAsync();
    }

    public async Task<WorkOrderDto?> GetByIdAsync(Guid id)
    {
        var workOrder = await _db.WorkOrders.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id);
        return workOrder is null ? null : MapToDto(workOrder);
    }

    public async Task<WorkOrderDto?> UpdateStatusAsync(Guid id, UpdateWorkOrderStatusRequest request)
    {
        if (!Enum.TryParse<WorkOrderStatus>(request.Status, true, out var status))
        {
            throw new InvalidOperationException("Invalid work order status value.");
        }

        var workOrder = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == id);
        if (workOrder is null)
        {
            return null;
        }

        workOrder.Status = status;
        await _db.SaveChangesAsync();

        return MapToDto(workOrder);
    }

    private static WorkOrderDto MapToDto(WorkOrder workOrder)
    {
        return new WorkOrderDto(
            workOrder.Id,
            workOrder.TicketId,
            workOrder.TenantId,
            workOrder.AssignedTo,
            workOrder.ScheduledAt,
            workOrder.Status.ToString(),
            workOrder.CreatedAt);
    }
}
