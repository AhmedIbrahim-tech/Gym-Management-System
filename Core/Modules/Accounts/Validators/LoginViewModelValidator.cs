namespace Core.Modules.Accounts.Validators;

public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
{
    public LoginViewModelValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email Is Required")
            .EmailAddress()
            .WithMessage("Invalid email address format.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password Is Required");
    }
}
