using Core.Mappers;
using Infrastructure.Entities.Sessions;
using Infrastructure.Entities.Users.GymUsers;

namespace Core.Modules.Sessions;

public interface ISessionService
{
    Task<IEnumerable<SessionViewModel>> GetAllSessionsAsync(CancellationToken cancellationToken = default);
    Task<SessionViewModel?> GetSessionByIDAsync(int id, CancellationToken cancellationToken = default);
    Task<UpdateSessionViewModel?> GetSessionToUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CreateSessionAsync(CreateSessionViewModel viewModel, CancellationToken cancellationToken = default);
    Task<bool> UpdateSessionAsync(int id, UpdateSessionViewModel viewModel, CancellationToken cancellationToken = default);
    Task<bool> RemoveSessionAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrainerSelectViewModel>> LoadTrainersDropDownAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CategorySelectViewModel>> LoadCategoriesDropDownAsync(CancellationToken cancellationToken = default);
}

public class SessionService(IUnitOfWork _unitOfWork, ISessionRepository _sessionRepository) : ISessionService
{
    #region Create Session

    public async Task<bool> CreateSessionAsync(CreateSessionViewModel viewModel, CancellationToken cancellationToken = default)
    {
        if (!IsValidSessionTime(viewModel.StartDate, viewModel.EndDate))
        {
            return false;
        }

        if (!await IsTrainerExistAsync(viewModel.TrainerId, cancellationToken))
        {
            return false;
        }

        if (!await IsCategoryExistAsync(viewModel.CategoryId, cancellationToken))
        {
            return false;
        }

        var session = viewModel.ToSession();
        await _unitOfWork.GetRepository<Session>().AddAsync(session, cancellationToken);
        return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
    }

    #endregion

    #region Get All Sessions

    public async Task<IEnumerable<SessionViewModel>> GetAllSessionsAsync(CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetAllSessionsWithTrainerAndCategoryAsync(cancellationToken);
        var sessionsList = sessions.OrderByDescending(s => s.StartDate).ToList();

        if (!sessionsList.Any())
        {
            return [];
        }

        var mappedSessions = sessionsList.Select(s => s.ToSessionViewModel()).ToList();

        foreach (var session in mappedSessions)
        {
            session.AvailableSlots = session.Capacity - await _sessionRepository.GetCountOfBookedSlotsAsync(session.Id, cancellationToken);
        }

        return mappedSessions;
    }

    #endregion

    #region Get Session By Id

    public async Task<SessionViewModel?> GetSessionByIDAsync(int id, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetSessionWithTrainerAndCategoryAsync(id, cancellationToken);
        if (session is null)
        {
            return null;
        }

        var mappedSession = session.ToSessionViewModel();
        mappedSession.AvailableSlots = mappedSession.Capacity - await _sessionRepository.GetCountOfBookedSlotsAsync(session.Id, cancellationToken);

        return mappedSession;
    }

    #endregion

    #region Update Session

    public async Task<bool> UpdateSessionAsync(int id, UpdateSessionViewModel viewModel, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.GetRepository<Session>().GetByIDAsync(id, cancellationToken);

        if (!await IsSessionAvailableForUpdateAsync(session, cancellationToken))
        {
            return false;
        }

        if (!await IsTrainerExistAsync(viewModel.TrainerId, cancellationToken))
        {
            return false;
        }

        session!.TrainerId = viewModel.TrainerId;
        session.StartDate = viewModel.StartDate;
        session.EndDate = viewModel.EndDate;
        session.Description = viewModel.Description;
        session.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.GetRepository<Session>().Update(session);
        return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
    }

    #endregion

    #region Get Session For Update

    public async Task<UpdateSessionViewModel?> GetSessionToUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.GetRepository<Session>().GetByIDAsync(id, cancellationToken);
        if (session is null)
        {
            return null;
        }

        return session.ToUpdateSessionViewModel();
    }

    #endregion

    #region Remove Session

    public async Task<bool> RemoveSessionAsync(int id, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.GetRepository<Session>().GetByIDAsync(id, cancellationToken);
        if (!await IsSessionAvailableForDeleteAsync(session, cancellationToken))
        {
            return false;
        }

        _unitOfWork.GetRepository<Session>().Delete(session!);
        return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
    }

    #endregion

    #region Load Trainers Dropdown

    public async Task<IEnumerable<TrainerSelectViewModel>> LoadTrainersDropDownAsync(CancellationToken cancellationToken = default)
    {
        var trainers = await _unitOfWork.GetRepository<Trainer>().GetAllAsync(null, cancellationToken);
        return trainers.Select(t => t.ToTrainerSelectViewModel());
    }

    #endregion

    #region Load Categories Dropdown

    public async Task<IEnumerable<CategorySelectViewModel>> LoadCategoriesDropDownAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.GetRepository<Category>().GetAllAsync(null, cancellationToken);
        return categories.Select(c => c.ToCategorySelectViewModel());
    }

    #endregion

    #region Helper Methods

    private bool IsValidSessionTime(DateTime startTime, DateTime endTime)
    {
        DateTime now = DateTime.UtcNow;
        return startTime < endTime && startTime > now && endTime > now;
    }

    private async Task<bool> IsTrainerExistAsync(int trainerId, CancellationToken cancellationToken = default)
    {
        var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIDAsync(trainerId, cancellationToken);
        return trainer != null;
    }

    private async Task<bool> IsCategoryExistAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.GetRepository<Category>().GetByIDAsync(categoryId, cancellationToken);
        return category != null;
    }

    private async Task<bool> IsSessionAvailableForUpdateAsync(Session? session, CancellationToken cancellationToken = default)
    {
        if (session is null)
        {
            return false;
        }

        if (!IsValidSessionTime(session.StartDate, session.EndDate))
        {
            return false;
        }

        if (session.StartDate <= DateTime.UtcNow && session.EndDate >= DateTime.UtcNow)
        {
            return false;
        }

        var bookedSlots = await _sessionRepository.GetCountOfBookedSlotsAsync(session.Id, cancellationToken);
        return bookedSlots == 0;
    }

    private async Task<bool> IsSessionAvailableForDeleteAsync(Session? session, CancellationToken cancellationToken = default)
    {
        if (session is null)
        {
            return false;
        }

        if (session.StartDate <= DateTime.Now && session.EndDate >= DateTime.Now)
        {
            return false;
        }

        if (session.EndDate < DateTime.UtcNow)
        {
            return true;
        }

        var bookedSlots = await _sessionRepository.GetCountOfBookedSlotsAsync(session.Id, cancellationToken);
        return bookedSlots == 0;
    }

    #endregion
}
