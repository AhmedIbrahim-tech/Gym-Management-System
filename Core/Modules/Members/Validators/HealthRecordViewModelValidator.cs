namespace Core.Modules.Members.Validators;

public class HealthRecordViewModelValidator : AbstractValidator<HealthRecordViewModel>
{
    public HealthRecordViewModelValidator()
    {
        RuleFor(x => x.Height)
            .GreaterThan(0.1m)
            .LessThanOrEqualTo(300)
            .WithMessage("Height must be greater than 0");

        RuleFor(x => x.Weight)
            .GreaterThan(0.1m)
            .LessThanOrEqualTo(500)
            .WithMessage("Weight must be greater than 0");

        RuleFor(x => x.BloodType)
            .NotEmpty()
            .WithMessage("Blood Type Is Required")
            .MaximumLength(3)
            .WithMessage("Blood type must be 3 characters or less");
    }
}