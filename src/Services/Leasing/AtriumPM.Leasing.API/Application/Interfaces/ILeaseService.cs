using AtriumPM.Leasing.API.Application.DTOs;

namespace AtriumPM.Leasing.API.Application.Interfaces;

public interface ILeaseService
{
    Task<LeaseDto> CreateAsync(CreateLeaseRequest request);
    Task<IReadOnlyList<LeaseDto>> GetAllAsync();
    Task<LeaseDto?> GetByIdAsync(Guid id);
    Task<LeaseDto?> UpdateStatusAsync(Guid id, UpdateLeaseStatusRequest request);
}
