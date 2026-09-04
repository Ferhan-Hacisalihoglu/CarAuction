using CarAuction.Application.DTOs.Users;
using FluentValidation;

namespace CarAuction.Application.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");
    }
}

public class UpdateUserStatusRequestValidator : AbstractValidator<UpdateUserStatusRequest>
{
    public UpdateUserStatusRequestValidator()
    {
        // No additional validation needed for boolean
    }
}

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("Role ID must be greater than 0");
    }
}
