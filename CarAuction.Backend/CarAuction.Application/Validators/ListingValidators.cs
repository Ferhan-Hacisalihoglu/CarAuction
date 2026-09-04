using CarAuction.Application.DTOs.Listings;
using FluentValidation;

namespace CarAuction.Application.Validators;

public class CreateListingRequestValidator : AbstractValidator<CreateListingRequest>
{
    public CreateListingRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255).WithMessage("Title cannot exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative");

        When(x => x.IsAuction, () =>
        {
            RuleFor(x => x.StartingPrice)
                .NotNull().WithMessage("Starting price is required for auction listings")
                .GreaterThan(0).WithMessage("Starting price must be greater than 0");

            RuleFor(x => x.StartTime)
                .NotNull().WithMessage("Start time is required for auction listings");

            RuleFor(x => x.EndTime)
                .NotNull().WithMessage("End time is required for auction listings")
                .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time");

            RuleFor(x => x.MinBidIncrement)
                .NotNull().WithMessage("Minimum bid increment is required for auction listings")
                .GreaterThan(0).WithMessage("Minimum bid increment must be greater than 0");
        });
    }
}

public class UpdateListingRequestValidator : AbstractValidator<UpdateListingRequest>
{
    public UpdateListingRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255).WithMessage("Title cannot exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative");
    }
}
