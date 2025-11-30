namespace Web.Controllers;

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Trainer},{Roles.Member}")]
public class BookingController(IBookingService _bookingService, IToastNotification _toastNotification) : Controller
{
    #region Get Bookings

    [RequirePermission(Permissions.BookingsView)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingService.GetAllSessionsAsync(cancellationToken);

        ViewData["Title"] = "Bookings";

        if (bookings == null || !bookings.Any())
        {
            ViewData["EmptyIcon"] = "calendar-x";
            ViewData["EmptyTitle"] = "No Sessions Available";
            ViewData["EmptyMessage"] = "Create training session first to get started";
        }

        return View(bookings);
    }

    #endregion

    #region Get Members For Session

    [RequirePermission(Permissions.BookingsView)]
    public async Task<IActionResult> GetMembersForUpcomingSession(int id, CancellationToken cancellationToken = default)
    {
        var members = await _bookingService.GetMembersForUpcomingBySessionIdAsync(id, cancellationToken);
        return View(members);
    }

    [RequirePermission(Permissions.BookingsView)]
    public async Task<IActionResult> GetMembersForOngoingSessions(int id, CancellationToken cancellationToken = default)
    {
        var members = await _bookingService.GetMembersForOngoingBySessionIdAsync(id, cancellationToken);
        return View(members);
    }

    #endregion

    #region Create Booking

    [RequirePermission(Permissions.BookingsCreate)]
    public async Task<IActionResult> Create(int id, CancellationToken cancellationToken = default)
    {
        var members = await _bookingService.GetMembersForDropDownAsync(id, cancellationToken);
        ViewBag.members = new SelectList(members, "Id", "Name");
        return View();
    }

    [HttpPost]
    [RequirePermission(Permissions.BookingsCreate)]
    public async Task<IActionResult> Create(CreateBookingViewModel createdBooking, CancellationToken cancellationToken = default)
    {
        var result = await _bookingService.CreateNewBookingAsync(createdBooking, cancellationToken);
        if (result)
        {
            _toastNotification.AddSuccessToastMessage("Booking Created successfully!");
        }
        else
        {
            _toastNotification.AddErrorToastMessage("Failed to Create Booking, Member does not have Active Plan.");
        }

        return RedirectToAction(nameof(GetMembersForUpcomingSession), new { id = createdBooking.SessionId });
    }

    #endregion

    #region Cancel Booking

    [HttpPost]
    [RequirePermission(Permissions.BookingsDelete)]
    public async Task<IActionResult> Cancel(int memberId, int sessionId, CancellationToken cancellationToken = default)
    {
        var result = await _bookingService.CancelBookingAsync(memberId, sessionId, cancellationToken);
        if (result)
        {
            _toastNotification.AddSuccessToastMessage("Booking cancelled successfully!");
        }
        else
        {
            _toastNotification.AddErrorToastMessage("Failed to cancel Booking.");
        }

        return RedirectToAction(nameof(GetMembersForUpcomingSession), new { id = sessionId });
    }

    #endregion

    #region Mark Attendance

    [HttpPost]
    [RequirePermission(Permissions.BookingsMarkAttendance)]
    public async Task<IActionResult> Attended(int memberId, int sessionId, CancellationToken cancellationToken = default)
    {
        await _bookingService.MemberAttendedAsync(memberId, sessionId, cancellationToken);
        return RedirectToAction(nameof(GetMembersForOngoingSessions), new { id = sessionId });
    }

    #endregion
}
