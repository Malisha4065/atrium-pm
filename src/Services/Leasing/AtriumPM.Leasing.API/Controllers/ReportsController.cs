using AtriumPM.Leasing.API.Application.DTOs;
using AtriumPM.Leasing.API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtriumPM.Leasing.API.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IOccupancyReportService _occupancyReportService;

    public ReportsController(IOccupancyReportService occupancyReportService)
    {
        _occupancyReportService = occupancyReportService;
    }

    [HttpGet("occupancy/summary")]
    public async Task<ActionResult<OccupancySummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _occupancyReportService.GetSummaryAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("occupancy/units")]
    public async Task<ActionResult<IReadOnlyList<UnitOccupancyDto>>> GetByUnit(CancellationToken cancellationToken)
    {
        var result = await _occupancyReportService.GetUnitOccupancyAsync(cancellationToken);
        return Ok(result);
    }
}
