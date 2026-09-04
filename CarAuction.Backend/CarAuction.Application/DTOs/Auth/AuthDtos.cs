namespace CarAuction.Application.DTOs.Auth;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password
);

public record LoginRequest(
    string Email,
    string Password
);

public record RefreshTokenRequest(
    string Token
);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    UserDto User
);

public record UserDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? RoleName
);
