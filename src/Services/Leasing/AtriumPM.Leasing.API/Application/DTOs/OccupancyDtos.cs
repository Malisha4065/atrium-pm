namespace AtriumPM.Leasing.API.Application.DTOs;

public record OccupancySummaryDto(int TotalLeases, int ActiveLeases, int DraftLeases, int ExpiredLeases, decimal OccupancyRate);

public record UnitOccupancyDto(Guid UnitId, int ActiveLeaseCount, DateTime? LastLeaseStartDate);
