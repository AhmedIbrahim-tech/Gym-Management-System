using Infrastructure.Entities.Users.Identity;
using Microsoft.AspNetCore.Identity;

namespace Web.Controllers;

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Trainer},{Roles.Member}")]
public class MemberController(
    IMemberService _memberService, 
    IToastNotification _toastNotification,
    UserManager<ApplicationUser> _userManager) : Controller
{
    #region Get Members

    [RequirePermission(Permissions.MembersView)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var currentUserId = currentUser?.Id;
        var members = await _memberService.GetAllMembersAsync(currentUserId, cancellationToken);

        ViewData["Title"] = "Members";
        ViewData["PageTitle"] = "Members List";
        ViewData["PageSubtitle"] = "Manage your gym members";
        ViewData["PageIcon"] = "people";
        ViewData["ShowActionButton"] = true;
        ViewData["ActionButtonText"] = "Add Member";
        ViewData["ActionButtonUrl"] = Url.Action("Create");
        ViewData["ActionButtonIcon"] = "person-plus";

        if (members == null || !members.Any())
        {
            ViewData["EmptyIcon"] = "people";
            ViewData["EmptyTitle"] = "No Members Available";
            ViewData["EmptyMessage"] = "Add your first member to get started";
        }

        return View(members);
    }

    [RequirePermission(Permissions.MembersView)]
    public async Task<IActionResult> MemberDetails(int id, CancellationToken cancellationToken = default)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var currentUserId = currentUser?.Id;
        var member = await _memberService.GetMemberDetailsAsync(id, currentUserId, cancellationToken);
        if (member == null)
        {
            _toastNotification.AddErrorToastMessage("Member Not Found!");
            return RedirectToAction(nameof(Index));
        }
        return View(member);
    }

    [RequirePermission(Permissions.MembersView)]
    public async Task<IActionResult> HealthRecordDetails(int id, CancellationToken cancellationToken = default)
    {
        var healthRecord = await _memberService.GetMemberHealthRecordAsync(id, cancellationToken);
        if (healthRecord == null)
        {
            _toastNotification.AddErrorToastMessage("Health Record is Not Found!");
            return RedirectToAction(nameof(Index));
        }
        return View(healthRecord);
    }

    #endregion

    #region Create Member

    [RequirePermission(Permissions.MembersCreate)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [RequirePermission(Permissions.MembersCreate)]
    public async Task<IActionResult> CreateMember(CreateMemberViewModel input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            _toastNotification.AddErrorToastMessage("Please check the form and fix any validation errors.");
            return View(nameof(Create), input);
        }

        if (input.FormFile == null || input.FormFile.Length == 0)
        {
            ModelState.AddModelError(nameof(input.FormFile), "Profile photo is required.");
            _toastNotification.AddErrorToastMessage("Profile photo is required.");
            return View(nameof(Create), input);
        }

        bool createMember = await _memberService.CreateMemberAsync(input, cancellationToken);

        if (createMember)
            _toastNotification.AddSuccessToastMessage("Member Created Successfully!");
        else
            _toastNotification.AddErrorToastMessage("Member Failed To Create. Email or Phone Number Already Exists, or Photo Upload Failed!");

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Update Member

    [RequirePermission(Permissions.MembersEdit)]
    public async Task<IActionResult> MemberEdit(int id, CancellationToken cancellationToken = default)
    {
        var member = await _memberService.GetMemberToUpdateAsync(id, cancellationToken);
        if (member == null)
        {
            _toastNotification.AddErrorToastMessage("Member Not Found!");
            return RedirectToAction(nameof(Index));
        }

        return View(member);
    }

    [HttpPost]
    [RequirePermission(Permissions.MembersEdit)]
    public async Task<IActionResult> MemberEdit([FromRoute] int id, MemberToUpdateViewModel input, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        bool updateMember = await _memberService.UpdateMemberAsync(id, input, cancellationToken);

        if (updateMember)
            _toastNotification.AddSuccessToastMessage("Member Updated Successfully!");
        else
            _toastNotification.AddErrorToastMessage("Member Failed To Update, Email or Phone Number Already Exists!");

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Toggle Status

    [HttpPost]
    [RequirePermission(Permissions.MembersEdit)]
    public async Task<IActionResult> ToggleStatus(int id, CancellationToken cancellationToken = default)
    {
        bool result = await _memberService.ToggleMemberStatusAsync(id, cancellationToken);

        if (result)
            _toastNotification.AddSuccessToastMessage("Member status updated successfully!");
        else
            _toastNotification.AddErrorToastMessage("Failed to update member status. Member may not have an active membership.");

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Delete Member

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    [RequirePermission(Permissions.MembersDelete)]
    public async Task<IActionResult> Delete([FromForm] int id, CancellationToken cancellationToken = default)
    {
        var result = await _memberService.RemoveMemberAsync(id, cancellationToken);

        if (result)
            _toastNotification.AddSuccessToastMessage("Member Deleted Successfully!");
        else
            _toastNotification.AddErrorToastMessage("Member Cannot be Deleted!");

        return RedirectToAction(nameof(Index));
    }

    #endregion
}
