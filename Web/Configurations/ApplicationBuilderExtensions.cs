using Infrastructure.Data;
using Infrastructure.Entities.Users.Identity;

namespace Web.Configurations;

public static class ApplicationBuilderExtensions
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DatabaseSeeding");

        try
        {
            var applicationDbContext = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            await DataSeeder.SeedDataAsync(applicationDbContext);
            await IdentitySeeding.SeedDataAsync(userManager, roleManager);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        // Global Exception Handling Middleware (should be early in pipeline)
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error/500");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        // Status Code Pages - Handle 404, 401, 403, etc. (must be after UseRouting)
        app.UseStatusCodePagesWithReExecute("/Error/{0}");

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication ConfigureRoutes(this WebApplication app)
    {
        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Account}/{action=Login}/{id?}")
            .WithStaticAssets();

        return app;
    }
}

