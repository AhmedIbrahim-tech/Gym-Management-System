using Core.Modules.UserManagement.ViewModels;
using Infrastructure.Entities.Users.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Web.ViewComponents;

public class UserInfoViewComponent : ViewComponent
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserInfoViewComponent(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = new CurrentUserViewModel();

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "No Role";

                model.FullName = $"{user.FirstName} {user.LastName}".Trim();
                if (string.IsNullOrEmpty(model.FullName))
                {
                    model.FullName = user.UserName ?? "User";
                }
                model.UserName = user.UserName ?? "User";
                model.PhotoUrl = !string.IsNullOrEmpty(user.PhotoUrl) 
                    ? $"/images/users/{user.PhotoUrl}" 
                    : "/assets/img/profiles/avator1.jpg";
                model.Role = role;
            }
        }

        // Store model in ViewData for access from parent views
        ViewData["CurrentUserViewModel"] = model;
        
        return View(model);
    }
}

