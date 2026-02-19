using AtriumPM.Leasing.API.Application.DTOs;
using AtriumPM.Leasing.API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtriumPM.Leasing.API.Controllers;

[ApiController]
[Authorize]
[Route("api/leases")]
public class LeasesController : ControllerBase
{
    private readonly ILeaseService _leaseService;

    public LeasesController(ILeaseService leaseService)
    {
        _leaseService = leaseService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LeaseDto>>> GetAll()
    {
        var result = await _leaseService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LeaseDto>> GetById(Guid id)
    {
        var result = await _leaseService.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<LeaseDto>> Create([FromBody] CreateLeaseRequest request)
    {
        var result = await _leaseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<LeaseDto>> UpdateStatus(Guid id, [FromBody] UpdateLeaseStatusRequest request)
    {
        var result = await _leaseService.UpdateStatusAsync(id, request);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
