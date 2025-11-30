namespace Web.Controllers;

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
public class PlanController(IPlanService _planService, IToastNotification _toastNotification) : Controller
{
    #region Get Plans

    [RequirePermission(Permissions.PlansView)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var plans = await _planService.GetAllPlansAsync(cancellationToken);

        ViewData["Title"] = "Plans";

        if (plans == null || !plans.Any())
        {
            ViewData["EmptyIcon"] = "inbox";
            ViewData["EmptyTitle"] = "No Plans Available";
            ViewData["EmptyMessage"] = "Create your first membership plan to get started";
        }

        return View(plans);
    }

    [RequirePermission(Permissions.PlansView)]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            _toastNotification.AddErrorToastMessage("Plan ID can Not be Zero or Negative!");
            return RedirectToAction(nameof(Index));
        }

        var plan = await _planService.GetPlanByIdAsync(id, cancellationToken);
        if (plan == null)
        {
            _toastNotification.AddErrorToastMessage("Plan Not Found!");
            return RedirectToAction(nameof(Index));
        }

        return View(plan);
    }

    #endregion

    #region Update Plan

    [RequirePermission(Permissions.PlansEdit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var plan = await _planService.GetPlanToUpdateAsync(id, cancellationToken);
        if (plan == null)
        {
            _toastNotification.AddErrorToastMessage("Plan Can not be Edited!");
            return RedirectToAction(nameof(Index));
        }

        return View(plan);
    }

    [HttpPost]
    [RequirePermission(Permissions.PlansEdit)]
    public async Task<IActionResult> Edit([FromRoute] int id, UpdatePlanViewModel input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        bool updatePlan = await _planService.UpdatePlanAsync(id, input, cancellationToken);

        if (updatePlan)
            _toastNotification.AddSuccessToastMessage("Plan Updated Successfully!");
        else
            _toastNotification.AddErrorToastMessage("Plan Failed To Update!");

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Toggle Status

    [HttpPost]
    [RequirePermission(Permissions.PlansEdit)]
    public async Task<IActionResult> ToggleStatus(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            _toastNotification.AddErrorToastMessage("Plan ID can Not be Zero or Negative!");
            return RedirectToAction(nameof(Index));
        }

        var activate = await _planService.ActivateAsync(id, cancellationToken);

        if (activate)
            _toastNotification.AddSuccessToastMessage("Plan status updated successfully!");
        else
            _toastNotification.AddErrorToastMessage("Failed to update plan status!");

        return RedirectToAction(nameof(Index));
    }

    #endregion
}
