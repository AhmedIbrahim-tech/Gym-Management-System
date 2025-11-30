using System.Security.Claims;
using Infrastructure.Constants;

namespace Infrastructure.Data.DataSeed;

public static class IdentitySeeding
{
    private const string DefaultPassword = "P@ssw0rd";

    public static async Task<bool> SeedDataAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        try
        {
            await SeedRolesAsync(roleManager);
            await SeedRolePermissionsAsync(roleManager);
            await SeedUsersAsync(userManager);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = Roles.GetAllRoles();

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole { Name = roleName };
                await roleManager.CreateAsync(role);
            }
        }
    }

    private static async Task SeedRolePermissionsAsync(RoleManager<IdentityRole> roleManager)
    {
        // SuperAdmin - All permissions
        var superAdminRole = await roleManager.FindByNameAsync(Roles.SuperAdmin);
        if (superAdminRole != null)
        {
            var existingClaims = await roleManager.GetClaimsAsync(superAdminRole);
            if (!existingClaims.Any(c => c.Type == PermissionConstants.PermissionClaimType))
            {
                var allPermissions = Permissions.GetAllPermissions();
                foreach (var permission in allPermissions)
                {
                    await roleManager.AddClaimAsync(superAdminRole, new Claim(PermissionConstants.PermissionClaimType, permission));
                }
            }
        }

        // Admin - Most permissions except roles management
        var adminRole = await roleManager.FindByNameAsync(Roles.Admin);
        if (adminRole != null)
        {
            var existingClaims = await roleManager.GetClaimsAsync(adminRole);
            if (!existingClaims.Any(c => c.Type == PermissionConstants.PermissionClaimType))
            {
                var adminPermissions = new List<string>
                {
                    Permissions.UsersView, Permissions.UsersCreate, Permissions.UsersEdit, Permissions.UsersDelete,
                    Permissions.MembersView, Permissions.MembersCreate, Permissions.MembersEdit, Permissions.MembersDelete,
                    Permissions.TrainersView, Permissions.TrainersCreate, Permissions.TrainersEdit, Permissions.TrainersDelete,
                    Permissions.PlansView, Permissions.PlansCreate, Permissions.PlansEdit, Permissions.PlansDelete,
                    Permissions.MembershipsView, Permissions.MembershipsCreate, Permissions.MembershipsEdit, Permissions.MembershipsDelete,
                    Permissions.SessionsView, Permissions.SessionsCreate, Permissions.SessionsEdit, Permissions.SessionsDelete,
                    Permissions.BookingsView, Permissions.BookingsCreate, Permissions.BookingsEdit, Permissions.BookingsDelete, Permissions.BookingsMarkAttendance,
                    Permissions.DashboardView, Permissions.AnalyticsView
                };
                foreach (var permission in adminPermissions)
                {
                    await roleManager.AddClaimAsync(adminRole, new Claim(PermissionConstants.PermissionClaimType, permission));
                }
            }
        }

        // Trainer - Limited permissions
        var trainerRole = await roleManager.FindByNameAsync(Roles.Trainer);
        if (trainerRole != null)
        {
            var existingClaims = await roleManager.GetClaimsAsync(trainerRole);
            if (!existingClaims.Any(c => c.Type == PermissionConstants.PermissionClaimType))
            {
                var trainerPermissions = new List<string>
                {
                    Permissions.MembersView,
                    Permissions.SessionsView, Permissions.SessionsCreate, Permissions.SessionsEdit,
                    Permissions.BookingsView, Permissions.BookingsCreate, Permissions.BookingsEdit, Permissions.BookingsDelete, Permissions.BookingsMarkAttendance,
                    Permissions.MembershipsView,
                    Permissions.DashboardView
                };
                foreach (var permission in trainerPermissions)
                {
                    await roleManager.AddClaimAsync(trainerRole, new Claim(PermissionConstants.PermissionClaimType, permission));
                }
            }
        }

        // Member - Very limited permissions
        var memberRole = await roleManager.FindByNameAsync(Roles.Member);
        if (memberRole != null)
        {
            var existingClaims = await roleManager.GetClaimsAsync(memberRole);
            if (!existingClaims.Any(c => c.Type == PermissionConstants.PermissionClaimType))
            {
                var memberPermissions = new List<string>
                {
                    Permissions.BookingsView, Permissions.BookingsCreate, Permissions.BookingsDelete,
                    Permissions.SessionsView,
                    Permissions.MembershipsView,
                    Permissions.DashboardView
                };
                foreach (var permission in memberPermissions)
                {
                    await roleManager.AddClaimAsync(memberRole, new Claim(PermissionConstants.PermissionClaimType, permission));
                }
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        if (await userManager.Users.AnyAsync())
        {
            return;
        }

        var usersToSeed = new[]
        {
            new UserSeedData
            {
                FirstName = "SuperAdmin",
                LastName = "SuperAdmin",
                UserName = "SuperAdmin",
                Email = "superadmin@vitagym.com",
                PhoneNumber = "1234567890",
                Role = Roles.SuperAdmin
            },
            new UserSeedData
            {
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "Admin",
                Email = "admin@vitagym.com",
                PhoneNumber = "1234567891",
                Role = Roles.Admin
            }
        };

        foreach (var userData in usersToSeed)
        {
            await CreateUserAsync(userManager, userData);
        }
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        UserSeedData userData)
    {
        var user = new ApplicationUser
        {
            FirstName = userData.FirstName,
            LastName = userData.LastName,
            UserName = userData.UserName,
            Email = userData.Email,
            PhoneNumber = userData.PhoneNumber,
            EmailConfirmed = true // Auto-confirm email for seeded users
        };

        var createResult = await userManager.CreateAsync(user, DefaultPassword);

        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(user, userData.Role);
        }
    }

    private class UserSeedData
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Role { get; set; }
    }
}
