using CarAuction.Application.DTOs.Auth;
using CarAuction.Application.Interfaces.Caching;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Enums;

namespace CarAuction.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ICacheService _cacheService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ICacheService cacheService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _cacheService = cacheService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("Email already registered");
        }

        // Resolve role from database instead of hardcoding
        var roleId = await _userRepository.GetRoleIdByNameAsync(UserRoles.User);
        if (roleId == null)
        {
            throw new InvalidOperationException($"Default role '{UserRoles.User}' not found in database");
        }

        // Generate salt and hash password
        var salt = _passwordHasher.GenerateSalt();
        var passwordHash = _passwordHasher.HashPassword(request.Password, salt);

        // Create user entity
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = passwordHash,
            Salt = salt,
            RoleId = roleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Save to database
        var userId = await _userRepository.CreateAsync(user);
        user.Id = userId;

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(userId, request.Email, UserRoles.User);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        // Update refresh token in database
        await _userRepository.UpdateRefreshTokenAsync(userId, refreshToken, refreshTokenExpiry);

        // Store session in Redis
        await _cacheService.SetStringAsync($"session:{userId}", refreshToken, TimeSpan.FromDays(7));

        // Build response
        return new AuthResponse(
            accessToken,
            refreshToken,
            new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                UserRoles.User
            )
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is deactivated");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.Salt, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Generate tokens
        var role = user.Role?.Name ?? UserRoles.User;
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, role);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        // Update refresh token in database
        await _userRepository.UpdateRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

        // Store session in Redis
        await _cacheService.SetStringAsync($"session:{user.Id}", refreshToken, TimeSpan.FromDays(7));

        // Build response
        return new AuthResponse(
            accessToken,
            refreshToken,
            new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                role
            )
        );
    }

    public async Task<AuthResponse> RefreshTokenAsync(string token)
    {
        // Check Redis grace period cache (10 seconds) to tolerate concurrent in-flight requests
        var cachedGraceResponse = await _cacheService.GetAsync<AuthResponse>($"refresh_grace:{token}");
        if (cachedGraceResponse != null)
        {
            return cachedGraceResponse;
        }

        // Find user by refresh token (no expiry filter in SQL - we check it in code)
        var user = await _userRepository.GetByRefreshTokenAsync(token);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Check if refresh token is expired
        if (user.RefreshTokenExpiry == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token expired");
        }

        // Generate new tokens (token rotation)
        var role = user.Role?.Name ?? UserRoles.User;
        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, role);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        // Update refresh token in database
        await _userRepository.UpdateRefreshTokenAsync(user.Id, newRefreshToken, refreshTokenExpiry);

        var response = new AuthResponse(
            newAccessToken,
            newRefreshToken,
            new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                role
            )
        );

        // Store old token in Redis Grace Cache for 10 seconds
        await _cacheService.SetAsync($"refresh_grace:{token}", response, TimeSpan.FromSeconds(10));

        // Update active session key in Redis
        await _cacheService.SetStringAsync($"session:{user.Id}", newRefreshToken, TimeSpan.FromDays(7));

        return response;
    }

    public async Task LogoutAsync(int userId)
    {
        await _userRepository.ClearRefreshTokenAsync(userId);
        await _cacheService.RemoveAsync($"session:{userId}");
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        return new UserDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role?.Name
        );
    }
}
