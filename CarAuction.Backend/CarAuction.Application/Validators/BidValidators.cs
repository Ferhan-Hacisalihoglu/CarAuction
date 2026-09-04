using CarAuction.Application.DTOs.Bids;
using FluentValidation;

namespace CarAuction.Application.Validators;

public class MakeOfferRequestValidator : AbstractValidator<MakeOfferRequest>
{
    public MakeOfferRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Offer amount must be greater than 0");
    }
}
