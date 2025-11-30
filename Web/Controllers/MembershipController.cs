namespace Web.Controllers;

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Trainer}")]
public class MembershipController(IMembershipService _membershipService, IToastNotification _toastNotification) : Controller
{
    #region Get Memberships

    [RequirePermission(Permissions.MembershipsView)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var memberships = await _membershipService.GetAllMemberShipsAsync(cancellationToken);

        ViewData["Title"] = "Memberships";
        ViewData["PageTitle"] = "Active Memberships";
        ViewData["PageSubtitle"] = "Members that have active plans";
        ViewData["PageIcon"] = "card-checklist";
        ViewData["ShowActionButton"] = true;
        ViewData["ActionButtonText"] = "New Membership";
        ViewData["ActionButtonUrl"] = Url.Action("Create");

        if (memberships == null || !memberships.Any())
        {
            ViewData["EmptyIcon"] = "card-checklist";
            ViewData["EmptyTitle"] = "No Active Memberships Available";
            ViewData["EmptyMessage"] = "No active memberships found";
        }

        return View(memberships);
    }

    #endregion

    #region Create Membership

    [RequirePermission(Permissions.MembershipsCreate)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = await BuildCreateViewModelAsync(cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [RequirePermission(Permissions.MembershipsCreate)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMembershipViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var viewModel = await BuildCreateViewModelAsync(cancellationToken);
            viewModel.PlanId = model.PlanId;
            viewModel.MemberId = model.MemberId;
            viewModel.StartDate = model.StartDate;
            return View(viewModel);
        }

        var createModel = new CreateMembershipViewModel
        {
            PlanId = model.PlanId,
            MemberId = model.MemberId,
            StartDate = model.StartDate
        };

        var result = await _membershipService.CreateMembershipAsync(createModel, cancellationToken);

        if (result)
        {
            _toastNotification.AddSuccessToastMessage("Membership created successfully!");
            return RedirectToAction(nameof(Index));
        }

        _toastNotification.AddErrorToastMessage("Failed to create membership. Member may already have an active membership or the selected plan/member is invalid.");
        
        var errorViewModel = await BuildCreateViewModelAsync(cancellationToken);
        errorViewModel.PlanId = model.PlanId;
        errorViewModel.MemberId = model.MemberId;
        errorViewModel.StartDate = model.StartDate;
        return View(errorViewModel);
    }

    #endregion

    #region Cancel Membership

    [HttpPost]
    [RequirePermission(Permissions.MembershipsDelete)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            _toastNotification.AddErrorToastMessage("Invalid membership ID.");
            return RedirectToAction(nameof(Index));
        }

        var result = await _membershipService.DeleteMemberShipAsync(id, cancellationToken);

        if (result)
        {
            _toastNotification.AddSuccessToastMessage("Membership cancelled successfully!");
        }
        else
        {
            _toastNotification.AddErrorToastMessage("Failed to cancel membership. Membership may not exist or is already inactive.");
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Helper Methods

    private async Task<CreateMembershipViewModel> BuildCreateViewModelAsync(CancellationToken cancellationToken = default)
    {
        var members = await _membershipService.GetMembersForDropDownAsync(cancellationToken);
        var plans = await _membershipService.GetPlansForDropDownAsync(cancellationToken);

        return new CreateMembershipViewModel
        {
            Members = members,
            Plans = plans
        };
    }

    #endregion
}
