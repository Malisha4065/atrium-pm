using AtriumPM.Leasing.API.Application.DTOs;
using AtriumPM.Leasing.API.Application.Events;
using AtriumPM.Leasing.API.Application.Interfaces;
using AtriumPM.Leasing.API.Domain.Entities;
using AtriumPM.Leasing.API.Domain.Enums;
using AtriumPM.Leasing.API.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AtriumPM.Leasing.API.Application.Services;

public class LeaseService : ILeaseService
{
    private readonly LeasingDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<LeaseService> _logger;

    public LeaseService(
        LeasingDbContext db,
        IPublishEndpoint publishEndpoint,
        ILogger<LeaseService> logger)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<LeaseDto> CreateAsync(CreateLeaseRequest request)
    {
        var lease = new Lease
        {
            Id = Guid.NewGuid(),
            UnitId = request.UnitId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ResidentName = request.ResidentName.Trim(),
            Status = LeaseStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _db.Leases.Add(lease);
        await _db.SaveChangesAsync();

        return MapToDto(lease);
    }

    public async Task<IReadOnlyList<LeaseDto>> GetAllAsync()
    {
        return await _db.Leases
            .AsNoTracking()
            .OrderByDescending(l => l.StartDate)
            .Select(l => new LeaseDto(l.Id, l.UnitId, l.TenantId, l.StartDate, l.EndDate, l.Status.ToString(), l.ResidentName, l.CreatedAt))
            .ToListAsync();
    }

    public async Task<LeaseDto?> GetByIdAsync(Guid id)
    {
        var lease = await _db.Leases.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        return lease is null ? null : MapToDto(lease);
    }

    public async Task<LeaseDto?> UpdateStatusAsync(Guid id, UpdateLeaseStatusRequest request)
    {
        if (!Enum.TryParse<LeaseStatus>(request.Status, true, out var parsedStatus))
        {
            throw new InvalidOperationException("Invalid lease status value.");
        }

        var lease = await _db.Leases.FirstOrDefaultAsync(l => l.Id == id);
        if (lease is null)
        {
            return null;
        }

        var previousStatus = lease.Status;
        lease.Status = parsedStatus;

        await _db.SaveChangesAsync();

        if (previousStatus != LeaseStatus.Active && parsedStatus == LeaseStatus.Active)
        {
            await _publishEndpoint.Publish(new LeaseSignedEvent(
                lease.Id,
                lease.TenantId,
                lease.UnitId,
                lease.StartDate,
                lease.EndDate,
                DateTime.UtcNow));

            _logger.LogInformation("Published LeaseSignedEvent for LeaseId {LeaseId}", lease.Id);
        }

        return MapToDto(lease);
    }

    private static LeaseDto MapToDto(Lease lease)
    {
        return new LeaseDto(
            lease.Id,
            lease.UnitId,
            lease.TenantId,
            lease.StartDate,
            lease.EndDate,
            lease.Status.ToString(),
            lease.ResidentName,
            lease.CreatedAt);
    }
}
