namespace Web.Attributes;

/// <summary>
/// Authorization attribute that requires the user to have a specific permission.
/// Usage: [RequirePermission(Permissions.UsersView)]
/// </summary>
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new ArgumentException("Permission cannot be null or empty", nameof(permission));
        }

        Policy = $"{PermissionConstants.PermissionPolicyPrefix}.{permission}";
    }
}

