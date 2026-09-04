using CarAuction.Application.DTOs.Groups;
using FluentValidation;

namespace CarAuction.Application.Validators;

public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Group name is required")
            .MaximumLength(100).WithMessage("Group name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
    }
}

public class SendGroupMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendGroupMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required")
            .MaximumLength(5000).WithMessage("Message cannot exceed 5000 characters");
    }
}
