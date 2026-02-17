using AtriumPM.Identity.API.Application.DTOs;
using AtriumPM.Identity.API.Application.Interfaces;
using AtriumPM.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtriumPM.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "TenantAdmin,SystemAdmin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITenantContext _tenantContext;

    public UsersController(IUserService userService, ITenantContext tenantContext)
    {
        _userService = userService;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Create a user within the current tenant scope.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] RegisterUserRequest request)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request, _tenantContext.TenantId);
            return CreatedAtAction(nameof(GetAll), null, user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all users in the current tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetUsersByTenantAsync(_tenantContext.TenantId);
        return Ok(users);
    }
}
