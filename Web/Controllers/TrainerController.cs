namespace Web.Controllers;

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
public class TrainerController(ITrainerService _trainerService, IToastNotification _toastNotification) : Controller
{
    #region Get Trainers

    [RequirePermission(Permissions.TrainersView)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var trainers = await _trainerService.GetAllTrainersAsync(cancellationToken);

        ViewData["Title"] = "Trainers";
        ViewData["PageTitle"] = "Trainers List";
        ViewData["PageSubtitle"] = "Manage your gym trainers";
        ViewData["PageIcon"] = "person-badge";
        ViewData["ShowActionButton"] = true;
        ViewData["ActionButtonText"] = "Add Trainer";
        ViewData["ActionButtonUrl"] = Url.Action("Create");

        if (trainers == null || !trainers.Any())
        {
            ViewData["EmptyIcon"] = "person-badge";
            ViewData["EmptyTitle"] = "No Trainers Available";
            ViewData["EmptyMessage"] = "Add your first trainer to get started";
        }

        return View(trainers);
    }

    [RequirePermission(Permissions.TrainersView)]
    public async Task<IActionResult> TrainerDetails(int id, CancellationToken cancellationToken = default)
    {
        var trainer = await _trainerService.GetTrainerDetailsAsync(id, cancellationToken);
        if (trainer == null)
        {
            _toastNotification.AddErrorToastMessage("Trainer Not Found!");
            return RedirectToAction(nameof(Index));
        }
        return View(trainer);
    }

    #endregion

    #region Create Trainer

    [RequirePermission(Permissions.TrainersCreate)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [RequirePermission(Permissions.TrainersCreate)]
    public async Task<IActionResult> CreateTrainer(CreateTrainerViewModel input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("DataMissed", "Check Missing Data!!");
            return View(nameof(Create), input);
        }

        bool createTrainer = await _trainerService.CreateTrainerAsync(input, cancellationToken);

        if (createTrainer)
            _toastNotification.AddSuccessToastMessage("Trainer Created Successfully!");
        else
            _toastNotification.AddErrorToastMessage("Trainer Failed To Create, Email or Phone Number Already Exists!");

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Update Trainer

    [RequirePermission(Permissions.TrainersEdit)]
    public async Task<IActionResult> TrainerEdit(int id, CancellationToken cancellationToken = default)
    {
        var trainer = await _trainerService.GetTrainerToUpdateAsync(id, cancellationToken);
        if (trainer == null)
        {
            _toastNotification.AddErrorToastMessage("Trainer Not Found!");
            return RedirectToAction(nameof(Index));
        }

        return View(trainer);
    }

    [HttpPost]
    [RequirePermission(Permissions.TrainersEdit)]
    public async Task<IActionResult> TrainerEdit([FromRoute] int id, TrainerToUpdateViewModel input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        bool updateTrainer = await _trainerService.UpdateTrainerAsync(id, input, cancellationToken);

        if (updateTrainer)
            _toastNotification.AddSuccessToastMessage("Trainer Updated Successfully!");
        else
            _toastNotification.AddErrorToastMessage("Trainer Failed To Update, Email or Phone Number Already Exists!");

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Delete Trainer

    [HttpPost]
    [Authorize(Roles = Roles.SuperAdmin)]
    [RequirePermission(Permissions.TrainersDelete)]
    public async Task<IActionResult> Delete([FromForm] int id, CancellationToken cancellationToken = default)
    {
        var result = await _trainerService.RemoveTrainerAsync(id, cancellationToken);

        if (result)
            _toastNotification.AddSuccessToastMessage("Trainer Deleted Successfully!");
        else
            _toastNotification.AddErrorToastMessage("Trainer Cannot be Deleted!");

        return RedirectToAction(nameof(Index));
    }

    #endregion
}
