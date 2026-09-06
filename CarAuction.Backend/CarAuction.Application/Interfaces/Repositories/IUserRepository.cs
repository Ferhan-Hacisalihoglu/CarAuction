using CarAuction.Application.DTOs.Auth;
using CarAuction.Domain.Entities;

namespace CarAuction.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<bool> EmailExistsAsync(string email);
    Task<int> CreateAsync(User user);
    Task UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiry);
    Task ClearRefreshTokenAsync(int userId);
    Task<int?> GetRoleIdByNameAsync(string roleName);
    Task<(List<User> Users, int Total)> GetPaginatedAsync(int page, int limit, string? search = null);
    Task UpdateProfileAsync(int userId, string firstName, string lastName);
    Task UpdateStatusAsync(int id, bool isActive);
    Task UpdateRoleAsync(int id, int roleId);
    Task<bool> RoleExistsAsync(int roleId);
    Task UpdatePasswordAsync(int userId, string newHash, string newSalt);
    Task<long> CountAsync();
    Task<long> CountRecentAsync(TimeSpan period);
    Task<List<User>> GetRecentAsync(int limit);
}
