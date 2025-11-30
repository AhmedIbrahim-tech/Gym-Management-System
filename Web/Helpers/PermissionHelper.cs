using Infrastructure.Constants;
using System.Security.Claims;

namespace Web.Helpers;

/// <summary>
/// Helper class for checking permissions in Razor views
/// </summary>
public static class PermissionHelper
{
    /// <summary>
    /// Checks if the current user has a specific permission
    /// </summary>
    public static bool HasPermission(this ClaimsPrincipal user, string permission)
    {
        if (user == null || string.IsNullOrWhiteSpace(permission))
            return false;

        // SuperAdmin has all permissions
        if (user.IsInRole(Roles.SuperAdmin))
            return true;

        // Check if user has the required permission claim
        return user.HasClaim(PermissionConstants.PermissionClaimType, permission);
    }
}

