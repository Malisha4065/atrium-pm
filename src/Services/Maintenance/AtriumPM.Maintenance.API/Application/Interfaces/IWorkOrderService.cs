using AtriumPM.Maintenance.API.Application.DTOs;

namespace AtriumPM.Maintenance.API.Application.Interfaces;

public interface IWorkOrderService
{
    Task<WorkOrderDto> CreateAsync(CreateWorkOrderRequest request);
    Task<IReadOnlyList<WorkOrderDto>> GetAllAsync();
    Task<WorkOrderDto?> GetByIdAsync(Guid id);
    Task<WorkOrderDto?> UpdateStatusAsync(Guid id, UpdateWorkOrderStatusRequest request);
}
