using CarAuction.Application.DTOs.Auth;

namespace CarAuction.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string token);
    Task LogoutAsync(int userId);
    Task<UserDto?> GetCurrentUserAsync(int userId);
}
