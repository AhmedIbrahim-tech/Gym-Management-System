using Infrastructure.Entities.Users.Identity;
using Microsoft.AspNetCore.Identity;

namespace Web.Controllers;

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Trainer}")]
public class SearchController : Controller
{
    private readonly IMemberService _memberService;
    private readonly ITrainerService _trainerService;
    private readonly ISessionService _sessionService;
    private readonly IMembershipService _membershipService;
    private readonly IUserManagementService _userManagementService;
    private readonly IPlanService _planService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SearchController(
        IMemberService memberService,
        ITrainerService trainerService,
        ISessionService sessionService,
        IMembershipService membershipService,
        IUserManagementService userManagementService,
        IPlanService planService,
        UserManager<ApplicationUser> userManager)
    {
        _memberService = memberService;
        _trainerService = trainerService;
        _sessionService = sessionService;
        _membershipService = membershipService;
        _userManagementService = userManagementService;
        _planService = planService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Search(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Json(new { success = false, message = "Search query is required" });
        }

        var results = new
        {
            success = true,
            query = query,
            results = new
            {
                members = await SearchMembersAsync(query, cancellationToken),
                trainers = await SearchTrainersAsync(query, cancellationToken),
                sessions = await SearchSessionsAsync(query, cancellationToken),
                memberships = await SearchMembershipsAsync(query, cancellationToken),
                users = await SearchUsersAsync(query, cancellationToken),
                plans = await SearchPlansAsync(query, cancellationToken)
            }
        };

        return Json(results);
    }

    private async Task<object[]> SearchMembersAsync(string query, CancellationToken cancellationToken)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var currentUserId = currentUser?.Id;
        var members = await _memberService.GetAllMembersAsync(currentUserId, cancellationToken);
        var searchTerm = query.ToLower();

        return members
            .Where(m => 
                m.Name?.ToLower().Contains(searchTerm) == true ||
                m.Email?.ToLower().Contains(searchTerm) == true ||
                m.Phone?.ToLower().Contains(searchTerm) == true)
            .Take(5)
            .Select(m => new
            {
                id = m.Id,
                name = m.Name,
                email = m.Email,
                type = "Member",
                url = Url.Action("MemberDetails", "Member", new { id = m.Id })
            })
            .ToArray();
    }

    private async Task<object[]> SearchTrainersAsync(string query, CancellationToken cancellationToken)
    {
        var trainers = await _trainerService.GetAllTrainersAsync(cancellationToken);
        var searchTerm = query.ToLower();

        return trainers
            .Where(t => 
                t.Name?.ToLower().Contains(searchTerm) == true ||
                t.Email?.ToLower().Contains(searchTerm) == true ||
                t.Phone?.ToLower().Contains(searchTerm) == true)
            .Take(5)
            .Select(t => new
            {
                id = t.Id,
                name = t.Name,
                email = t.Email,
                type = "Trainer",
                url = Url.Action("TrainerDetails", "Trainer", new { id = t.Id })
            })
            .ToArray();
    }

    private async Task<object[]> SearchSessionsAsync(string query, CancellationToken cancellationToken)
    {
        var sessions = await _sessionService.GetAllSessionsAsync(cancellationToken);
        var searchTerm = query.ToLower();

        return sessions
            .Where(s => 
                s.CategoryName?.ToLower().Contains(searchTerm) == true ||
                s.Description?.ToLower().Contains(searchTerm) == true ||
                s.TrainerName?.ToLower().Contains(searchTerm) == true)
            .Take(5)
            .Select(s => new
            {
                id = s.Id,
                name = s.CategoryName,
                description = s.Description,
                trainerName = s.TrainerName,
                type = "Session",
                url = Url.Action("Details", "Session", new { id = s.Id })
            })
            .ToArray();
    }

    private async Task<object[]> SearchMembershipsAsync(string query, CancellationToken cancellationToken)
    {
        var memberships = await _membershipService.GetAllMemberShipsAsync(cancellationToken);
        var searchTerm = query.ToLower();

        return memberships
            .Where(m => 
                m.MemberName?.ToLower().Contains(searchTerm) == true ||
                m.PlanName?.ToLower().Contains(searchTerm) == true)
            .Take(5)
            .Select(m => new
            {
                id = m.MemberId,
                name = m.MemberName,
                planName = m.PlanName,
                startDate = m.StartDate.ToShortDateString(),
                endDate = m.EndDate.ToShortDateString(),
                type = "Membership",
                url = Url.Action("Index", "Membership")
            })
            .ToArray();
    }

    private async Task<object[]> SearchUsersAsync(string query, CancellationToken cancellationToken)
    {
        var users = await _userManagementService.GetAllUsersAsync(cancellationToken);
        var searchTerm = query.ToLower();

        return users
            .Where(u => 
                u.FullName?.ToLower().Contains(searchTerm) == true ||
                u.Email?.ToLower().Contains(searchTerm) == true ||
                u.UserName?.ToLower().Contains(searchTerm) == true ||
                u.PhoneNumber?.ToLower().Contains(searchTerm) == true ||
                u.Role?.ToLower().Contains(searchTerm) == true)
            .Take(5)
            .Select(u => new
            {
                id = u.Id,
                name = u.FullName,
                email = u.Email,
                userName = u.UserName,
                role = u.Role,
                type = "User",
                url = Url.Action("Index", "UserManagement")
            })
            .ToArray();
    }

    private async Task<object[]> SearchPlansAsync(string query, CancellationToken cancellationToken)
    {
        var plans = await _planService.GetAllPlansAsync(cancellationToken);
        var searchTerm = query.ToLower();

        return plans
            .Where(p => 
                p.Name?.ToLower().Contains(searchTerm) == true ||
                p.Description?.ToLower().Contains(searchTerm) == true)
            .Take(5)
            .Select(p => new
            {
                id = p.Id,
                name = p.Name,
                description = p.Description,
                price = p.Price,
                durationDays = p.DurationDays,
                type = "Plan",
                url = Url.Action("Details", "Plan", new { id = p.Id })
            })
            .ToArray();
    }
}

