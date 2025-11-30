namespace Core.Modules.Plans.Validators;

public class UpdatePlanViewModelValidator : AbstractValidator<UpdatePlanViewModel>
{
    public UpdatePlanViewModelValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .Length(5, 200)
            .WithMessage("Description must be between 5 and 200 characters");

        RuleFor(x => x.DurationDays)
            .NotEmpty()
            .WithMessage("Duration is required")
            .InclusiveBetween(1, 365)
            .WithMessage("Duration must be between 1 and 365 days");

        RuleFor(x => x.Price)
            .NotEmpty()
            .WithMessage("Price is required")
            .GreaterThan(0.01m)
            .LessThanOrEqualTo(10000)
            .WithMessage("Price must be greater than 0");
    }
}