using AtriumPM.Identity.API.Application.DTOs;

namespace AtriumPM.Identity.API.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> CreateUserAsync(RegisterUserRequest request, Guid tenantId);
    Task<IEnumerable<UserDto>> GetUsersByTenantAsync(Guid tenantId);
}
