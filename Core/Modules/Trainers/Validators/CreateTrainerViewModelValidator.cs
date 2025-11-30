namespace Core.Modules.Trainers.Validators;

public class CreateTrainerViewModelValidator : AbstractValidator<CreateTrainerViewModel>
{
    public CreateTrainerViewModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .Length(2, 50)
            .WithMessage("Name must be between 2 and 50 characters.")
            .Matches(@"^[A-Za-z ]+$")
            .WithMessage("Name must contain only letters and spaces.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email address format.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(@"^(010|011|012|015)[0-9]{8}$")
            .WithMessage("Phone must start with 010, 011, 012, or 015 and be 11 digits long.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage("Date of birth is required.");

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage("Gender is required.");

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street name is required.")
            .Length(2, 150)
            .WithMessage("Street must be between 2 and 150 characters.")
            .Matches(@"^[A-Za-z0-9 ]+$")
            .WithMessage("Only letters, numbers, and spaces are allowed.");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City name is required.")
            .Length(2, 100)
            .WithMessage("City must be between 2 and 100 characters.")
            .Matches(@"^[A-Za-z ]+$")
            .WithMessage("Only letters and spaces are allowed.");

        RuleFor(x => x.BuildingNumber)
            .NotEmpty()
            .WithMessage("Building number is required.")
            .Must(bn => !string.IsNullOrWhiteSpace(bn) && bn.Length > 0)
            .WithMessage("Building number must be > 1 character.");

        RuleFor(x => x.Specialization)
            .NotNull()
            .WithMessage("Specialization is required.");
    }
}
