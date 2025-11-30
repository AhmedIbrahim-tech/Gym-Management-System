using System.ComponentModel.DataAnnotations;

namespace Core.Modules.UserManagement.ViewModels;

public class ProfileViewModel
{
    public string Id { get; set; } = null!;

    [Required(ErrorMessage = "First Name is required")]
    [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last Name is required")]
    [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Username is required")]
    [StringLength(256, ErrorMessage = "Username cannot exceed 256 characters")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Phone Number is required")]
    public string PhoneNumber { get; set; } = null!;

    public IFormFile? PhotoFile { get; set; }

    public string? CurrentPhotoUrl { get; set; }
}

