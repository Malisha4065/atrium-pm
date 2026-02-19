using AtriumPM.Maintenance.API.Application.DTOs;

namespace AtriumPM.Maintenance.API.Application.Interfaces;

public interface IMaintenanceTicketService
{
    Task<MaintenanceTicketDto> CreateAsync(CreateMaintenanceTicketRequest request);
    Task<IReadOnlyList<MaintenanceTicketDto>> GetAllAsync();
    Task<MaintenanceTicketDto?> GetByIdAsync(Guid id);
    Task<MaintenanceTicketDto?> UpdateStatusAsync(Guid id, UpdateMaintenanceTicketStatusRequest request);
}
