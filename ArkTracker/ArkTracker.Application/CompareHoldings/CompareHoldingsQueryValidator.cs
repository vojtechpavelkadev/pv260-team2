using FluentValidation;

namespace ArkTracker.Application.CompareHoldings;

public class CompareHoldingsQueryValidator 
    : AbstractValidator<CompareHoldingsQuery>
{
    public CompareHoldingsQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => (x.From.HasValue && x.To.HasValue) || (!x.From.HasValue && !x.To.HasValue))
            .WithMessage("Either provide both From and To, or none (latest 2 will be used)");

        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("From must be less than or equal to To");
    }
}