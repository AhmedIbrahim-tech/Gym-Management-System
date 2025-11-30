using Core.Modules.Accounts; using Core.Modules.Attendances; using Core.Modules.Bookings; using Core.Modules.Memberships; using Core.Modules.Payments; using Core.Modules.Plans; using Core.Modules.Sessions; using Core.Modules.Trainers; using Core.Modules.UserManagement; using Core.Modules.Analyticals; using Core.Modules.Members;
using Infrastructure.Entities.Users.Identity;

namespace Web.Controllers;

public class AccountController(IAccountService _accountService, SignInManager<ApplicationUser> _signInManager) : Controller
{
    #region Login

    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var user = await _accountService.ValidateUserAsync(input, cancellationToken);
        if (user == null)
        {
            // Check if user exists but is disabled
            var userByEmail = await _accountService.GetUserByEmailAsync(input.Email);
            if (userByEmail != null && !userByEmail.IsActive)
            {
                ModelState.AddModelError("InvalidLogin", "Your account has been disabled. Please contact the administrator.");
            }
            else
            {
                ModelState.AddModelError("InvalidLogin", "Email or Password is not Valid");
            }
            return View(input);
        }

        var result = await _signInManager.PasswordSignInAsync(user, input.Password, input.RememberMe, false);

        if (result.IsNotAllowed)
            ModelState.AddModelError("InvalidLogin", "User is not Allowed To Login");
        if (result.IsLockedOut)
            ModelState.AddModelError("InvalidLogin", "User is Locked out and not Allowed To Login");
        if (result.Succeeded)
            return RedirectToAction("Index", "Home");

        return View(input);
    }

    #endregion

    #region Logout

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    #endregion

    #region Access Denied

    public IActionResult AccessDenied()
    {
        return View();
    }

    #endregion
}
