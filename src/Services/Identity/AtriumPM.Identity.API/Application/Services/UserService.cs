using AtriumPM.Identity.API.Application.DTOs;
using AtriumPM.Identity.API.Application.Events;
using AtriumPM.Identity.API.Application.Interfaces;
using AtriumPM.Identity.API.Domain.Entities;
using AtriumPM.Identity.API.Domain.Enums;
using AtriumPM.Identity.API.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AtriumPM.Identity.API.Application.Services;

/// <summary>
/// Manages user CRUD within a tenant scope.
/// Publishes UserRegisteredEvent on creation.
/// </summary>
public class UserService : IUserService
{
    private readonly IdentityDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IdentityDbContext db,
        IPublishEndpoint publishEndpoint,
        ILogger<UserService> logger)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<UserDto> CreateUserAsync(RegisterUserRequest request, Guid tenantId)
    {
        // Check duplicate email within tenant
        var exists = await _db.Users
            .AnyAsync(u => u.Email == request.Email.ToLowerInvariant());

        if (exists)
            throw new InvalidOperationException($"Email '{request.Email}' is already registered in this tenant.");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            throw new ArgumentException($"Invalid role: {request.Role}");

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _logger.LogInformation("User {Email} created under tenant {TenantId} with role {Role}",
            user.Email, tenantId, role);

        await _publishEndpoint.Publish(new UserRegisteredEvent(
            user.Id, tenantId, user.Email, user.Role.ToString(), user.CreatedAt));

        return new UserDto(user.Id, user.TenantId, user.Email,
            user.FirstName, user.LastName, user.Role.ToString(),
            user.IsActive, user.CreatedAt);
    }

    public async Task<IEnumerable<UserDto>> GetUsersByTenantAsync(Guid tenantId)
    {
        return await _db.Users
            .AsNoTracking()
            .Select(u => new UserDto(u.Id, u.TenantId, u.Email,
                u.FirstName, u.LastName, u.Role.ToString(),
                u.IsActive, u.CreatedAt))
            .ToListAsync();
    }
}
