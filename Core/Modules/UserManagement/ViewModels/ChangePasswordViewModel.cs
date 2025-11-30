using System.ComponentModel.DataAnnotations;

namespace Core.Modules.UserManagement.ViewModels;

public class ChangePasswordViewModel
{
    public string UserId { get; set; } = null!;

    [Required(ErrorMessage = "New Password is required")]
    [StringLength(100, ErrorMessage = "Password must be at least 6 characters", MinimumLength = 6)]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Confirm Password is required")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = null!;
}

