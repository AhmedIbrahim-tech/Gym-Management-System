namespace Web.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure EmailSettings
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        
        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        // Services
        services.AddScoped<IAnalyticalService, AnalyticalService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<ITrainerService, TrainerService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRolePermissionsService, RolePermissionsService>();

        return services;
    }


    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
        {
            opt.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            options.RegisterPermissionPolicies();
        });

        return services;
    }

    public static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }

    public static IServiceCollection AddNToastNotifyConfiguration(this IServiceCollection services)
    {
        services.AddMvc().AddNToastNotifyToastr(new ToastrOptions
        {
            ProgressBar = true,
            PositionClass = ToastPositions.TopRight,
            PreventDuplicates = true,
            CloseButton = true,
            ShowDuration = 300,
            HideDuration = 1000,
            TimeOut = 5000,
            ExtendedTimeOut = 1000,
            ShowEasing = "swing",
            HideEasing = "linear",
            ShowMethod = "fadeIn",
            HideMethod = "fadeOut"
        });

        return services;
    }
}

