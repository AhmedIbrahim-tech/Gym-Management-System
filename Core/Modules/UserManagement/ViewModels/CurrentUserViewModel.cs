namespace Core.Modules.UserManagement.ViewModels;

public class CurrentUserViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = "/assets/img/profiles/avator1.jpg";
    public string Role { get; set; } = "Guest";
}

