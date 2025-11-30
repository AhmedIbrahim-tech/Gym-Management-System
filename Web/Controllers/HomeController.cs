namespace Web.Controllers;

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Trainer},{Roles.Member}")]
public class HomeController(IAnalyticalService _analyticalService) : Controller
{
    #region Dashboard

    [RequirePermission(Permissions.DashboardView)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var model = await _analyticalService.GetAnalyticalDataAsync(cancellationToken);
        return View(model);
    }

    #endregion
}
