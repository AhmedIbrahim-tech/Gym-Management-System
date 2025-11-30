using Infrastructure.Entities.Users.Identity;

namespace Core.Modules.Accounts;

public interface IAccountService
{
    Task<ApplicationUser?> ValidateUserAsync(LoginViewModel input, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
}

public class AccountService(UserManager<ApplicationUser> _userManager) : IAccountService
{
    #region Validate User

    public async Task<ApplicationUser?> ValidateUserAsync(LoginViewModel input, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user == null)
        {
            return null;
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return null;
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, input.Password);
        return isPasswordValid ? user : null;
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    #endregion
}

