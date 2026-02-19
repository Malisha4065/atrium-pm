using AtriumPM.Maintenance.API.Application.DTOs;
using AtriumPM.Maintenance.API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtriumPM.Maintenance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly IMaintenanceTicketService _ticketService;

    public TicketsController(IMaintenanceTicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MaintenanceTicketDto>>> GetAll()
    {
        var result = await _ticketService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MaintenanceTicketDto>> GetById(Guid id)
    {
        var result = await _ticketService.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<MaintenanceTicketDto>> Create([FromBody] CreateMaintenanceTicketRequest request)
    {
        var result = await _ticketService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<MaintenanceTicketDto>> UpdateStatus(Guid id, [FromBody] UpdateMaintenanceTicketStatusRequest request)
    {
        var result = await _ticketService.UpdateStatusAsync(id, request);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
