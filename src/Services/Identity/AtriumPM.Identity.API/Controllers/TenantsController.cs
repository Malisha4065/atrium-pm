using AtriumPM.Identity.API.Application.DTOs;
using AtriumPM.Identity.API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtriumPM.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// Register a new tenant with an admin user. Public endpoint.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterTenantRequest request)
    {
        try
        {
            var tenant = await _tenantService.RegisterTenantAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get a tenant by ID. Requires SystemAdmin role.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "SystemAdmin,TenantAdmin")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenant = await _tenantService.GetTenantByIdAsync(id);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    /// <summary>
    /// Get all tenants. Requires SystemAdmin role.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    [ProducesResponseType(typeof(IEnumerable<TenantDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var tenants = await _tenantService.GetAllTenantsAsync();
        return Ok(tenants);
    }
}
