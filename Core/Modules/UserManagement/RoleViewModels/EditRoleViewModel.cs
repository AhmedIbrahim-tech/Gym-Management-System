using System.ComponentModel.DataAnnotations;

namespace Core.Modules.UserManagement.RoleViewModels;

public class EditRoleViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 50 characters")]
    [Display(Name = "Role Name")]
    public string Name { get; set; } = string.Empty;
}

