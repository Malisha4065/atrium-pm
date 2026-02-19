using AtriumPM.Property.API.Application.DTOs;
using AtriumPM.Property.API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtriumPM.Property.API.Controllers;

[ApiController]
[Authorize]
[Route("api/units")]
public class UnitsController : ControllerBase
{
    private readonly IUnitService _unitService;

    public UnitsController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UnitDto>>> GetAll()
    {
        var result = await _unitService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UnitDto>> GetById(Guid id)
    {
        var result = await _unitService.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("building/{buildingId:guid}")]
    public async Task<ActionResult<IReadOnlyList<UnitDto>>> GetByBuilding(Guid buildingId)
    {
        var result = await _unitService.GetByBuildingIdAsync(buildingId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UnitDto>> Create([FromBody] CreateUnitRequest request)
    {
        var result = await _unitService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UnitDto>> Update(Guid id, [FromBody] UpdateUnitRequest request)
    {
        var result = await _unitService.UpdateAsync(id, request);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _unitService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
