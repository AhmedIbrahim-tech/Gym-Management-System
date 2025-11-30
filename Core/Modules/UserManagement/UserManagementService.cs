namespace Core.Modules.UserManagement;

public interface IUserManagementService
{
    Task<IEnumerable<UserListViewModel>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserListViewModel?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> CreateUserAsync(CreateUserViewModel viewModel, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserAsync(UpdateUserViewModel viewModel, CancellationToken cancellationToken = default);
    Task<bool> ChangeUserPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> ToggleUserStatusAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<string>> GetAvailableRolesAsync();
}

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailService _emailService;
    private readonly IAttachmentService _attachmentService;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailService emailService,
        IAttachmentService attachmentService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailService = emailService;
        _attachmentService = attachmentService;
    }

    public async Task<IEnumerable<UserListViewModel>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = _userManager.Users.ToList();
        var userViewModels = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "No Role";

            userViewModels.Add(new UserListViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Role = role,
                PhotoUrl = user.PhotoUrl,
                IsActive = user.IsActive
            });
        }

        return userViewModels;
    }

    public async Task<UserListViewModel?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "No Role";

        return new UserListViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Role = role,
            PhotoUrl = user.PhotoUrl,
            IsActive = user.IsActive
        };
    }

    public async Task<bool> CreateUserAsync(CreateUserViewModel viewModel, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if email already exists
            if (await _userManager.FindByEmailAsync(viewModel.Email) != null)
                return false;

            // Check if username already exists
            if (await _userManager.FindByNameAsync(viewModel.UserName) != null)
                return false;

            // Generate a random password
            var password = GenerateRandomPassword();

            // Handle photo upload
            string? photoUrl = null;
            if (viewModel.PhotoFile != null && viewModel.PhotoFile.Length > 0)
            {
                photoUrl = _attachmentService.Upload("users", viewModel.PhotoFile);
                if (string.IsNullOrEmpty(photoUrl))
                    return false;
            }

            var user = new ApplicationUser
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                UserName = viewModel.UserName,
                Email = viewModel.Email,
                PhoneNumber = viewModel.PhoneNumber,
                PhotoUrl = photoUrl,
                IsActive = viewModel.IsActive,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                // Delete uploaded photo if user creation failed
                if (!string.IsNullOrEmpty(photoUrl))
                    _attachmentService.Delete(photoUrl, "users");
                return false;
            }

            // Add user to role
            if (!string.IsNullOrEmpty(viewModel.Role) && await _roleManager.RoleExistsAsync(viewModel.Role))
            {
                await _userManager.AddToRoleAsync(user, viewModel.Role);
            }

            // Send email with credentials
            await _emailService.SendCredentialsEmailAsync(viewModel.Email, viewModel.UserName, password, cancellationToken);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> UpdateUserAsync(UpdateUserViewModel viewModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(viewModel.Id);
            if (user == null)
                return false;

            // Check if email is already taken by another user
            var existingUserByEmail = await _userManager.FindByEmailAsync(viewModel.Email);
            if (existingUserByEmail != null && existingUserByEmail.Id != viewModel.Id)
                return false;

            // Check if username is already taken by another user
            var existingUserByUsername = await _userManager.FindByNameAsync(viewModel.UserName);
            if (existingUserByUsername != null && existingUserByUsername.Id != viewModel.Id)
                return false;

            // Handle photo upload
            if (viewModel.PhotoFile != null && viewModel.PhotoFile.Length > 0)
            {
                var uploadedPhoto = _attachmentService.Upload("users", viewModel.PhotoFile);
                if (!string.IsNullOrEmpty(uploadedPhoto))
                {
                    // Delete old photo if exists
                    if (!string.IsNullOrEmpty(user.PhotoUrl))
                        _attachmentService.Delete(user.PhotoUrl, "users");

                    user.PhotoUrl = uploadedPhoto;
                }
            }

            // Update user properties
            user.FirstName = viewModel.FirstName;
            user.LastName = viewModel.LastName;
            user.UserName = viewModel.UserName;
            user.Email = viewModel.Email;
            user.PhoneNumber = viewModel.PhoneNumber;
            user.IsActive = viewModel.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return false;

            // Update role
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            if (!string.IsNullOrEmpty(viewModel.Role) && await _roleManager.RoleExistsAsync(viewModel.Role))
            {
                await _userManager.AddToRoleAsync(user, viewModel.Role);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ChangeUserPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Remove old password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
                return false;

            // Send email with new password
            if (!string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendPasswordChangedEmailAsync(user.Email, newPassword, cancellationToken);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ToggleUserStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Delete user photo if exists
            if (!string.IsNullOrEmpty(user.PhotoUrl))
                _attachmentService.Delete(user.PhotoUrl, "users");

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<List<string>> GetAvailableRolesAsync()
    {
        var roles = _roleManager.Roles.Select(r => r.Name!).ToList();
        return await Task.FromResult(roles);
    }

    private string GenerateRandomPassword(int length = 12)
    {
        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        var password = new char[length];

        for (int i = 0; i < length; i++)
        {
            password[i] = validChars[random.Next(validChars.Length)];
        }

        return new string(password);
    }
}

