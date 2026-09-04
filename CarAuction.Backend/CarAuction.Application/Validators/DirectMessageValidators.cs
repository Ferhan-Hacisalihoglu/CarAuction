using CarAuction.Application.DTOs.DirectMessages;
using FluentValidation;

namespace CarAuction.Application.Validators;

public class StartConversationRequestValidator : AbstractValidator<StartConversationRequest>
{
    public StartConversationRequestValidator()
    {
        RuleFor(x => x.TargetUserId)
            .GreaterThan(0).WithMessage("Target user ID must be greater than 0");
    }
}

public class SendDmMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendDmMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required")
            .MaximumLength(5000).WithMessage("Message cannot exceed 5000 characters");
    }
}
