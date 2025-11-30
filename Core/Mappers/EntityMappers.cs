using Core.Modules.Memberships.ViewModels;
using Infrastructure.Entities.Attendances;
using Infrastructure.Entities.Membership;
using Infrastructure.Entities.Sessions;
using Infrastructure.Entities.Users.GymUsers;

namespace Core.Mappers;

public static class EntityMappers
{
    #region Session Mappings

    public static SessionViewModel ToSessionViewModel(this Session session)
    {
        return new SessionViewModel
        {
            Id = session.Id,
            CategoryName = session.Category?.CategoryName ?? string.Empty,
            Description = session.Description,
            TrainerName = session.Trainer?.Name ?? string.Empty,
            StartDate = session.StartDate,
            EndDate = session.EndDate,
            Capacity = session.Capacity,
            AvailableSlots = 0 // This is set separately in services
        };
    }

    public static Session ToSession(this SessionViewModel viewModel)
    {
        return new Session
        {
            Id = viewModel.Id,
            Description = viewModel.Description,
            StartDate = viewModel.StartDate,
            EndDate = viewModel.EndDate,
            Capacity = viewModel.Capacity
        };
    }

    public static Session ToSession(this CreateSessionViewModel viewModel)
    {
        return new Session
        {
            Description = viewModel.Description,
            Capacity = viewModel.Capacity,
            StartDate = viewModel.StartDate,
            EndDate = viewModel.EndDate,
            TrainerId = viewModel.TrainerId,
            CategoryId = viewModel.CategoryId
        };
    }

    public static Session ToSession(this UpdateSessionViewModel viewModel, Session existingSession)
    {
        existingSession.Description = viewModel.Description;
        existingSession.StartDate = viewModel.StartDate;
        existingSession.EndDate = viewModel.EndDate;
        existingSession.TrainerId = viewModel.TrainerId;
        existingSession.UpdatedAt = DateTime.UtcNow;
        return existingSession;
    }

    public static UpdateSessionViewModel ToUpdateSessionViewModel(this Session session)
    {
        return new UpdateSessionViewModel
        {
            Description = session.Description,
            StartDate = session.StartDate,
            EndDate = session.EndDate,
            TrainerId = session.TrainerId
        };
    }

    #endregion

    #region Trainer Mappings

    public static TrainerSelectViewModel ToTrainerSelectViewModel(this Trainer trainer)
    {
        return new TrainerSelectViewModel
        {
            Id = trainer.Id,
            Name = trainer.Name
        };
    }

    #endregion

    #region Category Mappings

    public static CategorySelectViewModel ToCategorySelectViewModel(this Category category)
    {
        return new CategorySelectViewModel
        {
            Id = category.Id,
            Name = category.CategoryName
        };
    }

    #endregion

    #region Plan Mappings

    public static PlanSelectListViewModel ToPlanSelectListViewModel(this Plan plan)
    {
        return new PlanSelectListViewModel
        {
            Id = plan.Id,
            Name = plan.Name
        };
    }

    #endregion

    #region Member Mappings

    public static MemberSelectListViewModel ToMemberSelectListViewModel(this Member member)
    {
        return new MemberSelectListViewModel
        {
            Id = member.Id,
            Name = member.Name
        };
    }

    #endregion

    #region Membership Mappings

    public static MemberShipViewModel ToMemberShipViewModel(this MemberShip memberShip)
    {
        return new MemberShipViewModel
        {
            Id = memberShip.Id,
            MemberId = memberShip.MemberId,
            PlanId = memberShip.PlanId,
            MemberName = memberShip.Member?.Name ?? string.Empty,
            PlanName = memberShip.Plan?.Name ?? string.Empty,
            StartDate = memberShip.StartDate,
            EndDate = memberShip.EndDate,
            IsActive = memberShip.IsActive
        };
    }

    public static MemberShip ToMemberShip(this CreateMembershipViewModel viewModel)
    {
        return new MemberShip
        {
            MemberId = viewModel.MemberId,
            PlanId = viewModel.PlanId,
            StartDate = viewModel.StartDate ?? DateTime.Now,
            IsActive = true
        };
    }

    #endregion

    #region Attendance Mappings

    public static AttendanceViewModel ToAttendanceViewModel(this Attendance attendance)
    {
        return new AttendanceViewModel
        {
            Id = attendance.Id,
            MemberId = attendance.MemberId,
            MemberName = attendance.Member?.Name ?? string.Empty,
            CheckInTime = attendance.CheckInTime,
            CheckOutTime = null,
            Duration = "N/A"
        };
    }

    public static Attendance ToAttendance(this CreateAttendanceViewModel viewModel)
    {
        return new Attendance
        {
            MemberId = viewModel.MemberId,
            CheckInTime = DateTime.Now
        };
    }

    #endregion

    #region Payment Mappings

    public static PaymentViewModel ToPaymentViewModel(this Payment payment)
    {
        return new PaymentViewModel
        {
            Id = payment.Id,
            MemberId = payment.MemberId,
            MemberName = payment.Member?.Name ?? string.Empty,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            Method = payment.Method.ToString()
        };
    }

    public static Payment ToPayment(this CreatePaymentViewModel viewModel)
    {
        return new Payment
        {
            MemberId = viewModel.MemberId,
            Amount = viewModel.Amount,
            Method = Enum.Parse<PaymentMethod>(viewModel.Method),
            PaymentDate = DateTime.Now
        };
    }

    #endregion
}

