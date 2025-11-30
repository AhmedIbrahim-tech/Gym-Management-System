using Infrastructure.Entities.Membership;
using Infrastructure.Entities.Sessions;
using Infrastructure.Entities.Users.GymUsers;

namespace Core.Modules.Analyticals;

public interface IAnalyticalService
{
    Task<AnalyticalViewModel> GetAnalyticalDataAsync(CancellationToken cancellationToken = default);
}

public class AnalyticalService(IUnitOfWork _unitOfWork) : IAnalyticalService
{
    #region Get Analytical Data

    public async Task<AnalyticalViewModel> GetAnalyticalDataAsync(CancellationToken cancellationToken = default)
    {
        var activeMemberships = await _unitOfWork.GetRepository<MemberShip>().GetAllAsync(
            x => x.EndDate >= DateTime.Now, cancellationToken);
        var totalMembers = await _unitOfWork.GetRepository<Member>().GetAllAsync(null, cancellationToken);
        var totalTrainers = await _unitOfWork.GetRepository<Trainer>().GetAllAsync(null, cancellationToken);
        var upcomingSessions = await _unitOfWork.GetRepository<Session>().GetAllAsync(
            s => s.StartDate > DateTime.Now, cancellationToken);
        var ongoingSessions = await _unitOfWork.GetRepository<Session>().GetAllAsync(
            s => s.StartDate <= DateTime.Now && s.EndDate >= DateTime.Now, cancellationToken);
        var completedSessions = await _unitOfWork.GetRepository<Session>().GetAllAsync(
            s => s.EndDate < DateTime.Now, cancellationToken);

        return new AnalyticalViewModel
        {
            ActiveMembers = activeMemberships.Count(),
            TotalMembers = totalMembers.Count(),
            TotalTrainers = totalTrainers.Count(),
            UpcomingSessions = upcomingSessions.Count(),
            OngoingSessions = ongoingSessions.Count(),
            CompletedSessions = completedSessions.Count()
        };
    }

    #endregion
}
