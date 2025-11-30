namespace Web.Controllers;

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Trainer}")]
public class SessionController(ISessionService _sessionService, IToastNotification _toastNotification) : Controller
{
    #region Get Sessions

    [RequirePermission(Permissions.SessionsView)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionService.GetAllSessionsAsync(cancellationToken);

        ViewData["Title"] = "Sessions";

        if (sessions == null || !sessions.Any())
        {
            ViewData["EmptyIcon"] = "calendar-x";
            ViewData["EmptyTitle"] = "No Sessions Available";
            ViewData["EmptyMessage"] = "Create your first training session to get started";
        }

        return View(sessions);
    }

    [RequirePermission(Permissions.SessionsView)]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        var session = await _sessionService.GetSessionByIDAsync(id, cancellationToken);
        if (session == null)
        {
            _toastNotification.AddErrorToastMessage("Session is Not Found!");
            return RedirectToAction(nameof(Index));
        }
        return View(session);
    }

    #endregion

    #region Create Session

    [RequirePermission(Permissions.SessionsCreate)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        await LoadCategoriesDropDownAsync(cancellationToken);
        await LoadTrainersDropDownAsync(cancellationToken);
        return View();
    }

    [HttpPost]
    [RequirePermission(Permissions.SessionsCreate)]
    public async Task<IActionResult> Create(CreateSessionViewModel input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("DataMissed", "Check Missing Data!!");
            await LoadCategoriesDropDownAsync(cancellationToken);
            await LoadTrainersDropDownAsync(cancellationToken);
            return View(nameof(Create), input);
        }

        bool createSession = await _sessionService.CreateSessionAsync(input, cancellationToken);

        if (createSession)
            _toastNotification.AddSuccessToastMessage("Session Created Successfully!");
        else
        {
            _toastNotification.AddErrorToastMessage("Session Failed To Create!");
            await LoadCategoriesDropDownAsync(cancellationToken);
            await LoadTrainersDropDownAsync(cancellationToken);
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Update Session

    [RequirePermission(Permissions.SessionsEdit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            _toastNotification.AddErrorToastMessage("ID Can not be Zero or Negative!");
            return RedirectToAction(nameof(Index));
        }

        var session = await _sessionService.GetSessionToUpdateAsync(id, cancellationToken);
        if (session == null)
        {
            _toastNotification.AddErrorToastMessage("Session Can not be Edited!");
            return RedirectToAction(nameof(Index));
        }

        await LoadTrainersDropDownAsync(cancellationToken);
        return View(session);
    }

    [HttpPost]
    [RequirePermission(Permissions.SessionsEdit)]
    public async Task<IActionResult> Edit([FromRoute] int id, UpdateSessionViewModel input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            await LoadTrainersDropDownAsync(cancellationToken);
            return View(input);
        }

        bool updateSession = await _sessionService.UpdateSessionAsync(id, input, cancellationToken);

        if (updateSession)
            _toastNotification.AddSuccessToastMessage("Session Updated Successfully!");
        else
            _toastNotification.AddErrorToastMessage("Session Failed To Update!");

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Delete Session

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    [RequirePermission(Permissions.SessionsDelete)]
    public async Task<IActionResult> Delete([FromForm] int id, CancellationToken cancellationToken = default)
    {
        var result = await _sessionService.RemoveSessionAsync(id, cancellationToken);

        if (result)
            _toastNotification.AddSuccessToastMessage("Session Deleted Successfully!");
        else
            _toastNotification.AddErrorToastMessage("Session Cannot be Deleted!");

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Helper Methods

    private async Task LoadCategoriesDropDownAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _sessionService.LoadCategoriesDropDownAsync(cancellationToken);
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
    }

    private async Task LoadTrainersDropDownAsync(CancellationToken cancellationToken = default)
    {
        var trainers = await _sessionService.LoadTrainersDropDownAsync(cancellationToken);
        ViewBag.Trainers = new SelectList(trainers, "Id", "Name");
    }

    #endregion
}
