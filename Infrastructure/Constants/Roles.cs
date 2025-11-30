namespace Infrastructure.Constants;

public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Trainer = "Trainer";
    public const string Member = "Member";

    public static List<string> GetAllRoles()
    {
        return new List<string> { SuperAdmin, Admin, Trainer, Member };
    }
}

