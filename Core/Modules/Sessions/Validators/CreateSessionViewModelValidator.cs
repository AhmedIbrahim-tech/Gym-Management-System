namespace Core.Modules.Sessions.Validators;

public class CreateSessionViewModelValidator : AbstractValidator<CreateSessionViewModel>
{
    public CreateSessionViewModelValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .Length(10, 500)
            .WithMessage("Description must be between 10 and 500 characters");

        RuleFor(x => x.Capacity)
            .NotEmpty()
            .WithMessage("Capacity is required")
            .InclusiveBetween(0, 25)
            .WithMessage("Capacity must be between 0 and 25");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.TrainerId)
            .NotEmpty()
            .WithMessage("Trainer is required")
            .GreaterThan(0)
            .WithMessage("Trainer is required");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Category is required")
            .GreaterThan(0)
            .WithMessage("Category is required");
    }
}
