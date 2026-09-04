using CarAuction.Application.DTOs.Roles;
using FluentValidation;

namespace CarAuction.Application.Validators;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Role name can only contain letters, numbers, and underscores");
    }
}

public class AssignPermissionRequestValidator : AbstractValidator<AssignPermissionRequest>
{
    public AssignPermissionRequestValidator()
    {
        RuleFor(x => x.PermissionId)
            .GreaterThan(0).WithMessage("Permission ID must be greater than 0");
    }
}
