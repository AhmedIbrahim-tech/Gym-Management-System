using Core.Modules.UserManagement.ViewModels;
using Core.ThirdParty.AttachmentService;
using Infrastructure.Entities.Users.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NToastNotify;

namespace Web.Controllers;

[Authorize]
public class ProfileController(
    UserManager<ApplicationUser> userManager,
    IToastNotification toastNotification,
    IAttachmentService attachmentService) : Controller
{

    #region Profile

    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            toastNotification.AddErrorToastMessage("User not found!");
            return RedirectToAction("Index", "Home");
        }

        var model = new ProfileViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            CurrentPhotoUrl = user.PhotoUrl
        };

        ViewData["Title"] = "Profile";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Profile";
            return View(model);
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null || user.Id != model.Id)
        {
            toastNotification.AddErrorToastMessage("User not found!");
            return RedirectToAction("Index", "Home");
        }

        // Check if email is already taken by another user
        var existingUserByEmail = await userManager.FindByEmailAsync(model.Email);
        if (existingUserByEmail != null && existingUserByEmail.Id != model.Id)
        {
            ModelState.AddModelError("Email", "Email is already taken by another user.");
            ViewData["Title"] = "Profile";
            return View(model);
        }

        // Check if username is already taken by another user
        var existingUserByUsername = await userManager.FindByNameAsync(model.UserName);
        if (existingUserByUsername != null && existingUserByUsername.Id != model.Id)
        {
            ModelState.AddModelError("UserName", "Username is already taken by another user.");
            ViewData["Title"] = "Profile";
            return View(model);
        }

        // Handle photo upload
        if (model.PhotoFile != null && model.PhotoFile.Length > 0)
        {
            var uploadedPhoto = attachmentService.Upload("users", model.PhotoFile);
            if (!string.IsNullOrEmpty(uploadedPhoto))
            {
                // Delete old photo if exists
                if (!string.IsNullOrEmpty(user.PhotoUrl))
                    attachmentService.Delete(user.PhotoUrl, "users");

                user.PhotoUrl = uploadedPhoto;
            }
        }

        // Update user properties
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.UserName = model.UserName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ViewData["Title"] = "Profile";
            return View(model);
        }

        toastNotification.AddSuccessToastMessage("Profile updated successfully!");
        return RedirectToAction(nameof(Index));
    }

    #endregion
}

