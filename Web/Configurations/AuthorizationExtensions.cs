using Web.Authorization;

namespace Web.Configurations;

/// <summary>
/// Extension methods for configuring authorization policies
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Registers all permission-based authorization policies dynamically
    /// </summary>
    public static AuthorizationOptions RegisterPermissionPolicies(this AuthorizationOptions options)
    {
        var allPermissions = Permissions.GetAllPermissions();

        foreach (var permission in allPermissions)
        {
            var policyName = $"{PermissionConstants.PermissionPolicyPrefix}.{permission}";
            options.AddPolicy(policyName, policy =>
                policy.Requirements.Add(new PermissionRequirement(permission)));
        }

        return options;
    }
}

