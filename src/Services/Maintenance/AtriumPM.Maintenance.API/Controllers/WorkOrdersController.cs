using AtriumPM.Maintenance.API.Application.DTOs;
using AtriumPM.Maintenance.API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtriumPM.Maintenance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/work-orders")]
public class WorkOrdersController : ControllerBase
{
    private readonly IWorkOrderService _workOrderService;

    public WorkOrdersController(IWorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkOrderDto>>> GetAll()
    {
        var result = await _workOrderService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkOrderDto>> GetById(Guid id)
    {
        var result = await _workOrderService.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<WorkOrderDto>> Create([FromBody] CreateWorkOrderRequest request)
    {
        var result = await _workOrderService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<WorkOrderDto>> UpdateStatus(Guid id, [FromBody] UpdateWorkOrderStatusRequest request)
    {
        var result = await _workOrderService.UpdateStatusAsync(id, request);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
