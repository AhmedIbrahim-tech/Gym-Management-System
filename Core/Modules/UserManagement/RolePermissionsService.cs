using System.Security.Claims;

namespace Core.Modules.UserManagement;

public interface IRolePermissionsService
{
    Task<RolePermissionsViewModel?> GetRolePermissionsViewModelAsync(string roleId);
    Task<bool> UpdateRolePermissionsAsync(string roleId, List<string> selectedPermissions);
    Task<List<string>> GetRolePermissionsByRoleNameAsync(string roleName);
    Task<int> GetUsersCountByRoleAsync(string roleName);
    Task<bool> HasUsersAsync(string roleName);
}

public class RolePermissionsService(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager) : IRolePermissionsService
{
    public async Task<RolePermissionsViewModel?> GetRolePermissionsViewModelAsync(string roleId)
    {
        var role = await roleManager.FindByIdAsync(roleId);
        if (role == null)
            return null;

        var roleClaims = await roleManager.GetClaimsAsync(role);
        var selectedPermissions = roleClaims
            .Where(c => c.Type == PermissionConstants.PermissionClaimType)
            .Select(c => c.Value)
            .ToList();

        var permissionsByModule = Permissions.GetPermissionsByModule();
        var viewModel = new RolePermissionsViewModel
        {
            RoleId = role.Id,
            RoleName = role.Name ?? string.Empty,
            PermissionsByModule = new Dictionary<string, List<PermissionViewModel>>()
        };

        foreach (var module in permissionsByModule)
        {
            var permissionViewModels = module.Value.Select(permission =>
            {
                var displayName = permission.Replace("Permissions.", "").Replace(".", " ");
                return new PermissionViewModel
                {
                    Name = permission,
                    DisplayName = displayName,
                    IsSelected = selectedPermissions.Contains(permission)
                };
            }).ToList();

            viewModel.PermissionsByModule[module.Key] = permissionViewModels;
        }

        return viewModel;
    }

    public async Task<bool> UpdateRolePermissionsAsync(string roleId, List<string> selectedPermissions)
    {
        try
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
                return false;

            // Get current claims
            var currentClaims = await roleManager.GetClaimsAsync(role);
            var currentPermissions = currentClaims
                .Where(c => c.Type == PermissionConstants.PermissionClaimType)
                .ToList();

            // Remove all current permission claims
            foreach (var claim in currentPermissions)
            {
                await roleManager.RemoveClaimAsync(role, claim);
            }

            // Add selected permissions
            foreach (var permission in selectedPermissions)
            {
                var claim = new Claim(PermissionConstants.PermissionClaimType, permission);
                await roleManager.AddClaimAsync(role, claim);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<List<string>> GetRolePermissionsByRoleNameAsync(string roleName)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
            return new List<string>();

        var roleClaims = await roleManager.GetClaimsAsync(role);
        return roleClaims
            .Where(c => c.Type == PermissionConstants.PermissionClaimType)
            .Select(c => c.Value)
            .ToList();
    }

    public async Task<int> GetUsersCountByRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return 0;

        var usersInRole = await userManager.GetUsersInRoleAsync(roleName);
        return usersInRole.Count;
    }

    public async Task<bool> HasUsersAsync(string roleName)
    {
        var count = await GetUsersCountByRoleAsync(roleName);
        return count > 0;
    }
}

