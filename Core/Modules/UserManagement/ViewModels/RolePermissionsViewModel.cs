namespace Core.Modules.UserManagement.ViewModels;

public class RolePermissionsViewModel
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public Dictionary<string, List<PermissionViewModel>> PermissionsByModule { get; set; } = new();
}

public class PermissionViewModel
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

