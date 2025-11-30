using Core.Mappers;
using Infrastructure.Entities.Membership;
using Infrastructure.Entities.Sessions;
using Infrastructure.Entities.Users.GymUsers;

namespace Core.Modules.Bookings;

public interface IBookingService
{
	Task<IEnumerable<SessionViewModel>> GetAllSessionsAsync(CancellationToken cancellationToken = default);
	Task<IEnumerable<MemberForSessionViewModel>> GetMembersForUpcomingBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default);
	Task<IEnumerable<MemberForSessionViewModel>> GetMembersForOngoingBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default);
	Task<IEnumerable<MemberSelectListViewModel>> GetMembersForDropDownAsync(int sessionId, CancellationToken cancellationToken = default);
	Task<bool> CancelBookingAsync(int memberId, int sessionId, CancellationToken cancellationToken = default);
	Task<bool> CreateNewBookingAsync(CreateBookingViewModel createdBooking, CancellationToken cancellationToken = default);
	Task<bool> MemberAttendedAsync(int memberId, int sessionId, CancellationToken cancellationToken = default);
}

public class BookingService(IUnitOfWork _unitOfWork, IBookingRepository _bookingRepository, ISessionRepository _sessionRepository) : IBookingService
{
    #region Cancel Booking

    public async Task<bool> CancelBookingAsync(int memberId, int sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _unitOfWork.GetRepository<Session>().GetByIDAsync(sessionId, cancellationToken);
            if (session is null || session.StartDate <= DateTime.Now)
            {
                return false;
            }

            var bookings = await _unitOfWork.GetRepository<Booking>().GetAllAsync(
                x => x.SessionId == sessionId && x.MemberId == memberId, cancellationToken);
            var booking = bookings.FirstOrDefault();

            if (booking is null)
            {
                return false;
            }

            _bookingRepository.Delete(booking);
            return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Create New Booking

    public async Task<bool> CreateNewBookingAsync(CreateBookingViewModel createdBooking, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _unitOfWork.GetRepository<Session>().GetByIDAsync(createdBooking.SessionId, cancellationToken);
            if (session is null || session.StartDate <= DateTime.Now)
            {
                return false;
            }

            var memberships = await _unitOfWork.GetRepository<MemberShip>().GetAllAsync(
                x => x.MemberId == createdBooking.MemberId && x.EndDate >= DateTime.Now, cancellationToken);
            if (!memberships.Any())
            {
                return false;
            }

            var bookedSlots = await _sessionRepository.GetCountOfBookedSlotsAsync(createdBooking.SessionId, cancellationToken);
            var hasAvailableSlots = session.Capacity - bookedSlots;
            if (hasAvailableSlots == 0)
            {
                return false;
            }

            await _bookingRepository.AddAsync(new Booking
            {
                MemberId = createdBooking.MemberId,
                SessionId = createdBooking.SessionId,
                IsAttended = false
            }, cancellationToken);

            return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Get All Sessions

    public async Task<IEnumerable<SessionViewModel>> GetAllSessionsAsync(CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetAllSessionsWithTrainerAndCategoryAsync(cancellationToken);
        var upcomingSessions = sessions.Where(x => x.EndDate >= DateTime.Now)
            .OrderByDescending(x => x.StartDate)
            .ToList();

        if (!upcomingSessions.Any())
        {
            return [];
        }

        var mappedSessions = upcomingSessions.Select(s => s.ToSessionViewModel()).ToList();
        foreach (var item in mappedSessions)
        {
            item.AvailableSlots = item.Capacity - await _sessionRepository.GetCountOfBookedSlotsAsync(item.Id, cancellationToken);
        }

        return mappedSessions;
    }

    #endregion

    #region Get Members For Upcoming Session

    public async Task<IEnumerable<MemberForSessionViewModel>> GetMembersForUpcomingBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var membersForSession = await _bookingRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        return membersForSession.Select(x => new MemberForSessionViewModel
        {
            MemberId = x.MemberId,
            SessionId = sessionId,
            MemberName = x.Member.Name,
            BookingDate = x.CreatedAt.ToString()
        });
    }

    #endregion

    #region Get Members For Ongoing Session

    public async Task<IEnumerable<MemberForSessionViewModel>> GetMembersForOngoingBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var membersForSession = await _bookingRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        return membersForSession.Select(x => new MemberForSessionViewModel
        {
            MemberId = x.MemberId,
            SessionId = sessionId,
            MemberName = x.Member.Name,
            BookingDate = x.CreatedAt.ToString(),
            IsAttended = x.IsAttended
        });
    }

    #endregion

    #region Get Members For Dropdown

    public async Task<IEnumerable<MemberSelectListViewModel>> GetMembersForDropDownAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var bookings = await _unitOfWork.GetRepository<Booking>().GetAllAsync(
            x => x.SessionId == sessionId, cancellationToken);
        var bookedMemberIds = bookings.Select(x => x.MemberId).ToList();

        var availableMembers = await _unitOfWork.GetRepository<Member>().GetAllAsync(
            x => !bookedMemberIds.Contains(x.Id), cancellationToken);

        return availableMembers.Select(m => m.ToMemberSelectListViewModel());
    }

    #endregion

    #region Mark Member As Attended

    public async Task<bool> MemberAttendedAsync(int memberId, int sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var bookings = await _unitOfWork.GetRepository<Booking>().GetAllAsync(
                x => x.MemberId == memberId && x.SessionId == sessionId, cancellationToken);
            var memberSession = bookings.FirstOrDefault();

            if (memberSession is null)
            {
                return false;
            }

            memberSession.IsAttended = true;
            memberSession.UpdatedAt = DateTime.Now;
            _unitOfWork.GetRepository<Booking>().Update(memberSession);
            return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}

