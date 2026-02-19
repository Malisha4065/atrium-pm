using AtriumPM.Leasing.API.Application.DTOs;

namespace AtriumPM.Leasing.API.Application.Interfaces;

public interface IOccupancyReportService
{
    Task<OccupancySummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UnitOccupancyDto>> GetUnitOccupancyAsync(CancellationToken cancellationToken = default);
}
