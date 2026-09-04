using CarAuction.Application.DTOs.Users;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Domain.Entities;

namespace CarAuction.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PaginatedResponse<UserListResponse>> GetAllAsync(int page, int limit)
    {
        // Validate pagination parameters
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var (users, total) = await _userRepository.GetPaginatedAsync(page, limit);
        var totalPages = (int)Math.Ceiling((double)total / limit);

        return new PaginatedResponse<UserListResponse>(
            users.Select(u => MapToListResponse(u)).ToList(),
            total,
            page,
            limit,
            totalPages
        );
    }

    public async Task<UserListResponse?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return null;
        }

        return MapToListResponse(user);
    }

    public async Task<UserListResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        // Check if user exists first
        var existingUser = await _userRepository.GetByIdAsync(userId);
        if (existingUser == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        await _userRepository.UpdateProfileAsync(userId, request.FirstName, request.LastName);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found after update");
        }

        return MapToListResponse(user);
    }

    public async Task<UserListResponse> UpdateStatusAsync(int id, UpdateUserStatusRequest request)
    {
        // Check if user exists first
        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        await _userRepository.UpdateStatusAsync(id, request.IsActive);

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found after status update");
        }

        return MapToListResponse(user);
    }

    public async Task<UserListResponse> UpdateRoleAsync(int id, UpdateUserRoleRequest request)
    {
        // Check if user exists first
        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        // Check if role exists to avoid foreign key violation
        var roleExists = await _userRepository.RoleExistsAsync(request.RoleId);
        if (!roleExists)
        {
            throw new InvalidOperationException($"Role with ID {request.RoleId} does not exist");
        }

        await _userRepository.UpdateRoleAsync(id, request.RoleId);

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found after role update");
        }

        return MapToListResponse(user);
    }

    private static UserListResponse MapToListResponse(User user)
    {
        return new UserListResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role?.Name,
            user.IsActive,
            user.CreatedAt
        );
    }
}
