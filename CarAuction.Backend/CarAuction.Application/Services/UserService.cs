using CarAuction.Application.DTOs.Users;
using CarAuction.Application.Interfaces.Caching;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Domain.Entities;

namespace CarAuction.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepository, ICacheService cacheService, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
        _passwordHasher = passwordHasher;
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        // Verify current password
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.Salt, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect");
        }

        // Generate new hash
        var newSalt = _passwordHasher.GenerateSalt();
        var newHash = _passwordHasher.HashPassword(request.NewPassword, newSalt);

        // Update password
        await _userRepository.UpdatePasswordAsync(userId, newHash, newSalt);

        // Invalidate cache
        await _cacheService.RemoveAsync($"user:{userId}:profile");

        // Blacklist all tokens for this user so they re-authenticate with new password
        await _cacheService.SetStringAsync($"blacklist:user:{userId}", "password_changed", TimeSpan.FromHours(2));
    }

    public async Task<PaginatedResponse<UserListResponse>> GetAllAsync(int page, int limit, string? search = null)
    {
        // Validate pagination parameters
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var (users, total) = await _userRepository.GetPaginatedAsync(page, limit, search);
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
        return await _cacheService.GetOrSetAsync($"user:{id}:profile", async () =>
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? MapToListResponse(user) : null!;
        }, TimeSpan.FromMinutes(15));
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

        await _cacheService.RemoveAsync($"user:{userId}:profile");

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

        await _cacheService.RemoveAsync($"user:{id}:profile");

        // If deactivated, blacklist user instantly in Redis to revoke existing JWT access tokens
        if (!request.IsActive)
        {
            await _cacheService.SetStringAsync($"blacklist:user:{id}", "deactivated", TimeSpan.FromDays(7));
            await _cacheService.RemoveAsync($"session:{id}");
        }
        else
        {
            await _cacheService.RemoveAsync($"blacklist:user:{id}");
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

        await _cacheService.RemoveAsync($"user:{id}:profile");
        // Blacklist old tokens so user must re-authenticate with new role claims
        await _cacheService.SetStringAsync($"blacklist:user:{id}", "role_changed", TimeSpan.FromHours(2));

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
