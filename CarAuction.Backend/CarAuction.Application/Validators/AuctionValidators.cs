using CarAuction.Application.DTOs.Auctions;
using FluentValidation;

namespace CarAuction.Application.Validators;

public class PlaceBidRequestValidator : AbstractValidator<PlaceBidRequest>
{
    public PlaceBidRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Bid amount must be greater than 0");
    }
}
