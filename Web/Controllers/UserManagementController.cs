namespace Web.Controllers;

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
public class UserManagementController(IUserManagementService _userManagementService, IToastNotification _toastNotification) : Controller
{
    #region Get Users

    [RequirePermission(Permissions.UsersView)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var users = await _userManagementService.GetAllUsersAsync(cancellationToken);

        ViewData["Title"] = "Users";
        ViewData["PageTitle"] = "Users Management";
        ViewData["PageSubtitle"] = "Manage system users";
        ViewData["PageIcon"] = "users";
        ViewData["ShowActionButton"] = true;
        ViewData["ActionButtonText"] = "Add User";
        ViewData["ActionButtonUrl"] = Url.Action("Create");
        ViewData["ActionButtonIcon"] = "user-plus";

        if (users == null || !users.Any())
        {
            ViewData["EmptyIcon"] = "users";
            ViewData["EmptyTitle"] = "No Users Available";
            ViewData["EmptyMessage"] = "Add your first user to get started";
        }

        return View(users);
    }

    #endregion

    #region Create User

    [RequirePermission(Permissions.UsersCreate)]
    public async Task<IActionResult> Create()
    {
        var roles = await _userManagementService.GetAvailableRolesAsync();
        ViewBag.Roles = roles;
        return View();
    }

    [HttpPost]
    [RequirePermission(Permissions.UsersCreate)]
    public async Task<IActionResult> Create(CreateUserViewModel input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var roles = await _userManagementService.GetAvailableRolesAsync();
            ViewBag.Roles = roles;
            _toastNotification.AddErrorToastMessage("Please check the form and fix any validation errors.");
            return View(input);
        }

        bool result = await _userManagementService.CreateUserAsync(input, cancellationToken);

        if (result)
            _toastNotification.AddSuccessToastMessage("User Created Successfully! Credentials have been sent to the user's email.");
        else
            _toastNotification.AddErrorToastMessage("User creation failed. Email or Username may already exist.");

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Update User

    [RequirePermission(Permissions.UsersEdit)]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManagementService.GetUserByIdAsync(id, cancellationToken);
        if (user == null)
        {
            _toastNotification.AddErrorToastMessage("User Not Found!");
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userManagementService.GetAvailableRolesAsync();
        ViewBag.Roles = roles;

        var updateViewModel = new UpdateUserViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            UserName = user.UserName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            CurrentPhotoUrl = user.PhotoUrl,
            IsActive = user.IsActive
        };

        return View(updateViewModel);
    }

    [HttpPost]
    [RequirePermission(Permissions.UsersEdit)]
    public async Task<IActionResult> Edit(UpdateUserViewModel input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var roles = await _userManagementService.GetAvailableRolesAsync();
            ViewBag.Roles = roles;
            _toastNotification.AddErrorToastMessage("Please check the form and fix any validation errors.");
            return View(input);
        }

        bool result = await _userManagementService.UpdateUserAsync(input, cancellationToken);

        if (result)
            _toastNotification.AddSuccessToastMessage("User Updated Successfully!");
        else
            _toastNotification.AddErrorToastMessage("User update failed. Email or Username may already exist.");

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Change Password

    [RequirePermission(Permissions.UsersEdit)]
    public async Task<IActionResult> ChangePassword(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManagementService.GetUserByIdAsync(id, cancellationToken);
        if (user == null)
        {
            _toastNotification.AddErrorToastMessage("User Not Found!");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new ChangePasswordViewModel
        {
            UserId = user.Id
        };

        return View(viewModel);
    }

    [HttpPost]
    [RequirePermission(Permissions.UsersEdit)]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            _toastNotification.AddErrorToastMessage("Please check the form and fix any validation errors.");
            return View(input);
        }

        bool result = await _userManagementService.ChangeUserPasswordAsync(input.UserId, input.NewPassword, cancellationToken);

        if (result)
            _toastNotification.AddSuccessToastMessage("Password Changed Successfully! New password has been sent to the user's email.");
        else
            _toastNotification.AddErrorToastMessage("Password change failed.");

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Toggle Status

    [HttpPost]
    [RequirePermission(Permissions.UsersEdit)]
    public async Task<IActionResult> ToggleStatus(string id, CancellationToken cancellationToken = default)
    {
        bool result = await _userManagementService.ToggleUserStatusAsync(id, cancellationToken);

        if (result)
            _toastNotification.AddSuccessToastMessage("User status updated successfully!");
        else
            _toastNotification.AddErrorToastMessage("Failed to update user status.");

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Delete User

    [HttpPost]
    [Authorize(Roles = Roles.SuperAdmin)]
    [RequirePermission(Permissions.UsersDelete)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
    {
        bool result = await _userManagementService.DeleteUserAsync(id, cancellationToken);

        if (result)
            _toastNotification.AddSuccessToastMessage("User deleted successfully!");
        else
            _toastNotification.AddErrorToastMessage("Failed to delete user.");

        return RedirectToAction(nameof(Index));
    }

    #endregion
}

