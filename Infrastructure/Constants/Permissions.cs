namespace Infrastructure.Constants;

public static class Permissions
{
    // User Management Permissions
    public const string UsersView = "Permissions.Users.View";
    public const string UsersCreate = "Permissions.Users.Create";
    public const string UsersEdit = "Permissions.Users.Edit";
    public const string UsersDelete = "Permissions.Users.Delete";

    // Roles & Permissions Management
    public const string RolesView = "Permissions.Roles.View";
    public const string RolesCreate = "Permissions.Roles.Create";
    public const string RolesEdit = "Permissions.Roles.Edit";
    public const string RolesDelete = "Permissions.Roles.Delete";
    public const string RolesManagePermissions = "Permissions.Roles.ManagePermissions";

    // Member Management
    public const string MembersView = "Permissions.Members.View";
    public const string MembersCreate = "Permissions.Members.Create";
    public const string MembersEdit = "Permissions.Members.Edit";
    public const string MembersDelete = "Permissions.Members.Delete";

    // Trainer Management
    public const string TrainersView = "Permissions.Trainers.View";
    public const string TrainersCreate = "Permissions.Trainers.Create";
    public const string TrainersEdit = "Permissions.Trainers.Edit";
    public const string TrainersDelete = "Permissions.Trainers.Delete";

    // Plan Management
    public const string PlansView = "Permissions.Plans.View";
    public const string PlansCreate = "Permissions.Plans.Create";
    public const string PlansEdit = "Permissions.Plans.Edit";
    public const string PlansDelete = "Permissions.Plans.Delete";

    // Membership Management
    public const string MembershipsView = "Permissions.Memberships.View";
    public const string MembershipsCreate = "Permissions.Memberships.Create";
    public const string MembershipsEdit = "Permissions.Memberships.Edit";
    public const string MembershipsDelete = "Permissions.Memberships.Delete";

    // Session Management
    public const string SessionsView = "Permissions.Sessions.View";
    public const string SessionsCreate = "Permissions.Sessions.Create";
    public const string SessionsEdit = "Permissions.Sessions.Edit";
    public const string SessionsDelete = "Permissions.Sessions.Delete";

    // Booking Management
    public const string BookingsView = "Permissions.Bookings.View";
    public const string BookingsCreate = "Permissions.Bookings.Create";
    public const string BookingsEdit = "Permissions.Bookings.Edit";
    public const string BookingsDelete = "Permissions.Bookings.Delete";
    public const string BookingsMarkAttendance = "Permissions.Bookings.MarkAttendance";

    // Dashboard & Analytics
    public const string DashboardView = "Permissions.Dashboard.View";
    public const string AnalyticsView = "Permissions.Analytics.View";

    // Get all permissions
    public static List<string> GetAllPermissions()
    {
        return new List<string>
        {
            UsersView, UsersCreate, UsersEdit, UsersDelete,
            RolesView, RolesCreate, RolesEdit, RolesDelete, RolesManagePermissions,
            MembersView, MembersCreate, MembersEdit, MembersDelete,
            TrainersView, TrainersCreate, TrainersEdit, TrainersDelete,
            PlansView, PlansCreate, PlansEdit, PlansDelete,
            MembershipsView, MembershipsCreate, MembershipsEdit, MembershipsDelete,
            SessionsView, SessionsCreate, SessionsEdit, SessionsDelete,
            BookingsView, BookingsCreate, BookingsEdit, BookingsDelete, BookingsMarkAttendance,
            DashboardView, AnalyticsView
        };
    }

    // Get permissions grouped by module
    public static Dictionary<string, List<string>> GetPermissionsByModule()
    {
        return new Dictionary<string, List<string>>
        {
            {
                "User Management",
                new List<string> { UsersView, UsersCreate, UsersEdit, UsersDelete }
            },
            {
                "Roles & Permissions",
                new List<string> { RolesView, RolesCreate, RolesEdit, RolesDelete, RolesManagePermissions }
            },
            {
                "Members",
                new List<string> { MembersView, MembersCreate, MembersEdit, MembersDelete }
            },
            {
                "Trainers",
                new List<string> { TrainersView, TrainersCreate, TrainersEdit, TrainersDelete }
            },
            {
                "Plans",
                new List<string> { PlansView, PlansCreate, PlansEdit, PlansDelete }
            },
            {
                "Memberships",
                new List<string> { MembershipsView, MembershipsCreate, MembershipsEdit, MembershipsDelete }
            },
            {
                "Sessions",
                new List<string> { SessionsView, SessionsCreate, SessionsEdit, SessionsDelete }
            },
            {
                "Bookings",
                new List<string> { BookingsView, BookingsCreate, BookingsEdit, BookingsDelete, BookingsMarkAttendance }
            },
            {
                "Dashboard & Analytics",
                new List<string> { DashboardView, AnalyticsView }
            }
        };
    }
}

