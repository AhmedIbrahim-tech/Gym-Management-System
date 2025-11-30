using Core.Mappers;
using Infrastructure.Entities.Attendances;
using Infrastructure.Entities.Membership;
using Infrastructure.Entities.Users.GymUsers;

namespace Core.Modules.Attendances;

public interface IAttendanceService
{
    Task<bool> CheckInAsync(CreateAttendanceViewModel viewModel, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceViewModel>> GetAllAttendancesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceViewModel>> GetMemberAttendancesAsync(int memberId, CancellationToken cancellationToken = default);
    Task<AttendanceViewModel?> GetAttendanceByIdAsync(int id, CancellationToken cancellationToken = default);
}

public class AttendanceService(IUnitOfWork _unitOfWork) : IAttendanceService
{
    #region Check In

    public async Task<bool> CheckInAsync(CreateAttendanceViewModel viewModel, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if member exists
            var member = await _unitOfWork.GetRepository<Member>().GetByIDAsync(viewModel.MemberId, cancellationToken);
            if (member is null)
            {
                return false;
            }

            // Check if member has active membership
            var activeMembership = await _unitOfWork.GetRepository<MemberShip>().GetAllAsync(
                ms => ms.MemberId == viewModel.MemberId && ms.EndDate >= DateTime.Now, cancellationToken);
            if (!activeMembership.Any())
            {
                return false;
            }


            var attendance = viewModel.ToAttendance();
            await _unitOfWork.GetRepository<Attendance>().AddAsync(attendance, cancellationToken);
            return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
        }
        catch
        {
            return false;
        }
    }

    #endregion


    #region Get All Attendances

    public async Task<IEnumerable<AttendanceViewModel>> GetAllAttendancesAsync(CancellationToken cancellationToken = default)
    {
        var attendances = await _unitOfWork.GetRepository<Attendance>().GetAllAsync(
            null, cancellationToken, a => a.Member);
        var attendancesList = attendances.OrderByDescending(a => a.CheckInTime).ToList();

        if (!attendancesList.Any())
        {
            return [];
        }

        return attendancesList.Select(a => a.ToAttendanceViewModel());
    }

    #endregion

    #region Get Member Attendances

    public async Task<IEnumerable<AttendanceViewModel>> GetMemberAttendancesAsync(int memberId, CancellationToken cancellationToken = default)
    {
        var attendances = await _unitOfWork.GetRepository<Attendance>().GetAllAsync(
            a => a.MemberId == memberId, cancellationToken, a => a.Member);
        var attendancesList = attendances.OrderByDescending(a => a.CheckInTime).ToList();

        if (!attendancesList.Any())
        {
            return [];
        }

        return attendancesList.Select(a => a.ToAttendanceViewModel());
    }

    #endregion

    #region Get Attendance By Id

    public async Task<AttendanceViewModel?> GetAttendanceByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var attendance = await _unitOfWork.GetRepository<Attendance>().GetByIDAsync(id, cancellationToken);
        if (attendance is null)
        {
            return null;
        }

        return attendance.ToAttendanceViewModel();
    }

    #endregion

}

