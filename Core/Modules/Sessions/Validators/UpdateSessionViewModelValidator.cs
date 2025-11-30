namespace Core.Modules.Sessions.Validators;

public class UpdateSessionViewModelValidator : AbstractValidator<UpdateSessionViewModel>
{
    public UpdateSessionViewModelValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .Length(10, 500)
            .WithMessage("Description must be between 10 and 500 characters");

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
    }
}