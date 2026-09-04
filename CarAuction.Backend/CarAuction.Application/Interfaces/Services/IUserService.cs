using CarAuction.Application.DTOs.Users;

namespace CarAuction.Application.Interfaces.Services;

public interface IUserService
{
    Task<PaginatedResponse<UserListResponse>> GetAllAsync(int page, int limit);
    Task<UserListResponse?> GetByIdAsync(int id);
    Task<UserListResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<UserListResponse> UpdateStatusAsync(int id, UpdateUserStatusRequest request);
    Task<UserListResponse> UpdateRoleAsync(int id, UpdateUserRoleRequest request);
}
