namespace CarAuction.Application.DTOs.Roles;

public record CreateRoleRequest(
    string Name
);

public record RoleResponse(
    int Id,
    string Name
);

public record PermissionResponse(
    int Id,
    string Name,
    string? Description
);

public record AssignPermissionRequest(
    int PermissionId
);
