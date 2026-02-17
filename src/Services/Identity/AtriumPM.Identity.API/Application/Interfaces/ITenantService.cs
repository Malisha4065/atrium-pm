using AtriumPM.Identity.API.Application.DTOs;

namespace AtriumPM.Identity.API.Application.Interfaces;

public interface ITenantService
{
    Task<TenantDto> RegisterTenantAsync(RegisterTenantRequest request);
    Task<TenantDto?> GetTenantByIdAsync(Guid id);
    Task<IEnumerable<TenantDto>> GetAllTenantsAsync();
}
