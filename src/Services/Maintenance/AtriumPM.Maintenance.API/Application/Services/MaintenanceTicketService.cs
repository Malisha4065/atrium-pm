using AtriumPM.Maintenance.API.Application.DTOs;
using AtriumPM.Maintenance.API.Application.Interfaces;
using AtriumPM.Maintenance.API.Domain.Entities;
using AtriumPM.Maintenance.API.Domain.Enums;
using AtriumPM.Maintenance.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AtriumPM.Maintenance.API.Application.Services;

public class MaintenanceTicketService : IMaintenanceTicketService
{
    private readonly MaintenanceDbContext _db;

    public MaintenanceTicketService(MaintenanceDbContext db)
    {
        _db = db;
    }

    public async Task<MaintenanceTicketDto> CreateAsync(CreateMaintenanceTicketRequest request)
    {
        if ((request.UnitId is null && request.BuildingId is null) ||
            (request.UnitId is not null && request.BuildingId is not null))
        {
            throw new InvalidOperationException("Ticket must target exactly one of UnitId or BuildingId.");
        }

        if (!Enum.TryParse<TicketPriority>(request.Priority, true, out var priority))
        {
            throw new InvalidOperationException("Invalid ticket priority value.");
        }

        var ticket = new MaintenanceTicket
        {
            Id = Guid.NewGuid(),
            UnitId = request.UnitId,
            BuildingId = request.BuildingId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = priority,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        return MapToDto(ticket);
    }

    public async Task<IReadOnlyList<MaintenanceTicketDto>> GetAllAsync()
    {
        return await _db.Tickets
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new MaintenanceTicketDto(
                t.Id,
                t.UnitId,
                t.BuildingId,
                t.TenantId,
                t.Title,
                t.Description,
                t.Priority.ToString(),
                t.Status.ToString(),
                t.CreatedAt))
            .ToListAsync();
    }

    public async Task<MaintenanceTicketDto?> GetByIdAsync(Guid id)
    {
        var ticket = await _db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        return ticket is null ? null : MapToDto(ticket);
    }

    public async Task<MaintenanceTicketDto?> UpdateStatusAsync(Guid id, UpdateMaintenanceTicketStatusRequest request)
    {
        if (!Enum.TryParse<TicketStatus>(request.Status, true, out var status))
        {
            throw new InvalidOperationException("Invalid ticket status value.");
        }

        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
        if (ticket is null)
        {
            return null;
        }

        ticket.Status = status;
        await _db.SaveChangesAsync();

        return MapToDto(ticket);
    }

    private static MaintenanceTicketDto MapToDto(MaintenanceTicket ticket)
    {
        return new MaintenanceTicketDto(
            ticket.Id,
            ticket.UnitId,
            ticket.BuildingId,
            ticket.TenantId,
            ticket.Title,
            ticket.Description,
            ticket.Priority.ToString(),
            ticket.Status.ToString(),
            ticket.CreatedAt);
    }
}
