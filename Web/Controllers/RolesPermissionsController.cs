namespace Web.Controllers;

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
public class RolesPermissionsController(
    RoleManager<IdentityRole> _roleManager,
    IRolePermissionsService _rolePermissionsService,
    IToastNotification _toastNotification) : Controller
{
    [RequirePermission(Permissions.RolesView)]
    public async Task<IActionResult> Index()
    {
        var roles = _roleManager.Roles.ToList();
        
        var roleViewModels = new List<RoleViewModel>();
        foreach (var role in roles)
        {
            var roleName = role.Name ?? string.Empty;
            var usersCount = await _rolePermissionsService.GetUsersCountByRoleAsync(roleName);
            var isProtectedRole = roleName == Roles.SuperAdmin;
            var hasUsers = usersCount > 0;

            roleViewModels.Add(new RoleViewModel
            {
                Id = role.Id,
                Name = roleName,
                CreatedDate = DateTime.Now,
                IsActive = true,
                UsersCount = usersCount,
                CanEdit = !isProtectedRole,
                CanDelete = !isProtectedRole && !hasUsers
            });
        }

        ViewData["Title"] = "Roles & Permissions";
        ViewData["PageTitle"] = "Roles & Permissions";
        ViewData["PageSubtitle"] = "Manage your roles";
        ViewData["PageIcon"] = "shield";
        ViewData["ShowActionButton"] = true;
        ViewData["ActionButtonText"] = "Add Role";
        ViewData["ActionButtonUrl"] = Url.Action("Create");
        ViewData["ActionButtonIcon"] = "shield-plus";

        if (roleViewModels.Count == 0)
        {
            ViewData["EmptyIcon"] = "shield";
            ViewData["EmptyTitle"] = "No Roles Available";
            ViewData["EmptyMessage"] = "Add your first role to get started";
        }

        return View(roleViewModels);
    }

    [RequirePermission(Permissions.RolesCreate)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [RequirePermission(Permissions.RolesCreate)]
    public async Task<IActionResult> Create(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            _toastNotification.AddErrorToastMessage("Role name is required!");
            return View();
        }

        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (roleExists)
        {
            _toastNotification.AddErrorToastMessage("Role already exists!");
            return View();
        }

        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        if (result.Succeeded)
        {
            _toastNotification.AddSuccessToastMessage("Role created successfully!");
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            _toastNotification.AddErrorToastMessage(error.Description);
        }

        return View();
    }

    #region View Role Details

    [RequirePermission(Permissions.RolesView)]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            _toastNotification.AddErrorToastMessage("Role ID is required!");
            return RedirectToAction(nameof(Index));
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            _toastNotification.AddErrorToastMessage("Role not found!");
            return RedirectToAction(nameof(Index));
        }

        var roleName = role.Name ?? string.Empty;
        var usersCount = await _rolePermissionsService.GetUsersCountByRoleAsync(roleName);
        var permissions = await _rolePermissionsService.GetRolePermissionsByRoleNameAsync(roleName);

        var viewModel = new RoleViewModel
        {
            Id = role.Id,
            Name = roleName,
            CreatedDate = DateTime.Now,
            IsActive = true,
            UsersCount = usersCount,
            CanEdit = roleName != Roles.SuperAdmin,
            CanDelete = roleName != Roles.SuperAdmin && usersCount == 0
        };

        ViewData["Title"] = "Role Details";
        ViewData["PageTitle"] = $"Role Details - {roleName}";
        ViewData["PageSubtitle"] = "View role information and permissions";
        ViewData["PageIcon"] = "shield";
        ViewData["Permissions"] = permissions;
        ViewData["PermissionsCount"] = permissions.Count;

        return View(viewModel);
    }

    #endregion

    #region Edit Role

    [RequirePermission(Permissions.RolesEdit)]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            _toastNotification.AddErrorToastMessage("Role ID is required!");
            return RedirectToAction(nameof(Index));
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            _toastNotification.AddErrorToastMessage("Role not found!");
            return RedirectToAction(nameof(Index));
        }

        // Prevent editing protected roles
        if (role.Name == Roles.SuperAdmin)
        {
            _toastNotification.AddErrorToastMessage("Cannot edit SuperAdmin role!");
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = "Edit Role";
        ViewData["PageTitle"] = "Edit Role";
        ViewData["PageSubtitle"] = "Update role information";
        ViewData["PageIcon"] = "shield-edit";

        var viewModel = new EditRoleViewModel
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty
        };

        return View(viewModel);
    }

    [HttpPost]
    [RequirePermission(Permissions.RolesEdit)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditRoleViewModel viewModel)
    {
        if (string.IsNullOrEmpty(viewModel.Id))
        {
            _toastNotification.AddErrorToastMessage("Role ID is required!");
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(viewModel.Name))
        {
            _toastNotification.AddErrorToastMessage("Role name is required!");
            return RedirectToAction(nameof(Edit), new { id = viewModel.Id });
        }

        var role = await _roleManager.FindByIdAsync(viewModel.Id);
        if (role == null)
        {
            _toastNotification.AddErrorToastMessage("Role not found!");
            return RedirectToAction(nameof(Index));
        }

        // Prevent editing protected roles
        if (role.Name == Roles.SuperAdmin)
        {
            _toastNotification.AddErrorToastMessage("Cannot edit SuperAdmin role!");
            return RedirectToAction(nameof(Index));
        }

        // Check if new name already exists (excluding current role)
        if (role.Name != viewModel.Name)
        {
            var roleExists = await _roleManager.RoleExistsAsync(viewModel.Name);
            if (roleExists)
            {
                _toastNotification.AddErrorToastMessage("Role name already exists!");
                return RedirectToAction(nameof(Edit), new { id = viewModel.Id });
            }
        }

        role.Name = viewModel.Name;
        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            _toastNotification.AddSuccessToastMessage("Role updated successfully!");
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            _toastNotification.AddErrorToastMessage(error.Description);
        }

        return RedirectToAction(nameof(Edit), new { id = viewModel.Id });
    }

    #endregion

    #region Delete Role

    [HttpPost]
    [Authorize(Roles = Roles.SuperAdmin)]
    [RequirePermission(Permissions.RolesDelete)]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            _toastNotification.AddErrorToastMessage("Role ID is required!");
            return RedirectToAction(nameof(Index));
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            _toastNotification.AddErrorToastMessage("Role not found!");
            return RedirectToAction(nameof(Index));
        }

        // Prevent deleting protected roles
        if (role.Name == Roles.SuperAdmin)
        {
            _toastNotification.AddErrorToastMessage("Cannot delete SuperAdmin role!");
            return RedirectToAction(nameof(Index));
        }

        // Check if role has users
        var hasUsers = await _rolePermissionsService.HasUsersAsync(role.Name ?? string.Empty);
        if (hasUsers)
        {
            var usersCount = await _rolePermissionsService.GetUsersCountByRoleAsync(role.Name ?? string.Empty);
            _toastNotification.AddErrorToastMessage($"Cannot delete role. There are {usersCount} user(s) assigned to this role. Please reassign users to another role first.");
            return RedirectToAction(nameof(Index));
        }

        var result = await _roleManager.DeleteAsync(role);
        if (result.Succeeded)
        {
            _toastNotification.AddSuccessToastMessage("Role deleted successfully!");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                _toastNotification.AddErrorToastMessage(error.Description);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    [HttpPost]
    [RequirePermission(Permissions.RolesEdit)]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            _toastNotification.AddErrorToastMessage("Role ID is required!");
            return RedirectToAction(nameof(Index));
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            _toastNotification.AddErrorToastMessage("Role not found!");
            return RedirectToAction(nameof(Index));
        }

        // Note: IdentityRole doesn't have IsActive property
        // This is a placeholder for future implementation if needed
        _toastNotification.AddInfoToastMessage("Role status toggle is not implemented. Roles are always active.");

        return RedirectToAction(nameof(Index));
    }

    #region Manage Permissions

    [Authorize(Roles = Roles.SuperAdmin)]
    [RequirePermission(Permissions.RolesManagePermissions)]
    public async Task<IActionResult> ManagePermissions(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            _toastNotification.AddErrorToastMessage("Role ID is required!");
            return RedirectToAction(nameof(Index));
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            _toastNotification.AddErrorToastMessage("Role not found!");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = await _rolePermissionsService.GetRolePermissionsViewModelAsync(id);
        if (viewModel == null)
        {
            _toastNotification.AddErrorToastMessage("Failed to load role permissions!");
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = "Manage Permissions";
        ViewData["PageTitle"] = $"Manage Permissions - {role.Name}";
        ViewData["PageSubtitle"] = "Assign permissions to this role";
        ViewData["PageIcon"] = "shield-check";

        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Roles = Roles.SuperAdmin)]
    [RequirePermission(Permissions.RolesManagePermissions)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManagePermissions(string roleId, List<string> selectedPermissions)
    {
        if (string.IsNullOrEmpty(roleId))
        {
            _toastNotification.AddErrorToastMessage("Role ID is required!");
            return RedirectToAction(nameof(Index));
        }

        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            _toastNotification.AddErrorToastMessage("Role not found!");
            return RedirectToAction(nameof(Index));
        }

        // SuperAdmin role should always have all permissions
        if (role.Name == Roles.SuperAdmin)
        {
            selectedPermissions = Permissions.GetAllPermissions();
        }

        var result = await _rolePermissionsService.UpdateRolePermissionsAsync(roleId, selectedPermissions ?? new List<string>());

        if (result)
        {
            _toastNotification.AddSuccessToastMessage("Permissions updated successfully!");
        }
        else
        {
            _toastNotification.AddErrorToastMessage("Failed to update permissions!");
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion
}

