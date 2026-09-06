namespace CarAuction.Application.DTOs.Users;

public record UpdateProfileRequest(
    string FirstName,
    string LastName
);

public record UpdateUserStatusRequest(
    bool IsActive
);

public record UpdateUserRoleRequest(
    int RoleId
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

public record UserListResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? RoleName,
    bool IsActive,
    DateTime CreatedAt
);

public record PaginatedResponse<T>(
    List<T> Data,
    int Total,
    int Page,
    int Limit,
    int TotalPages
);
