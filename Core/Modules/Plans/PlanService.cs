using Infrastructure.Entities.Membership;

namespace Core.Modules.Plans;

public interface IPlanService
{
    Task<bool> UpdatePlanAsync(int planID, UpdatePlanViewModel viewModel, CancellationToken cancellationToken = default);
    Task<UpdatePlanViewModel?> GetPlanToUpdateAsync(int planID, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlanViewModel>> GetAllPlansAsync(CancellationToken cancellationToken = default);
    Task<PlanViewModel?> GetPlanByIdAsync(int planID, CancellationToken cancellationToken = default);
    Task<bool> ActivateAsync(int planID, CancellationToken cancellationToken = default);
}

public class PlanService(IUnitOfWork _unitOfWork) : IPlanService
{
    #region Activate/Deactivate Plan

    public async Task<bool> ActivateAsync(int planID, CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.GetRepository<Plan>().GetByIDAsync(planID, cancellationToken);
        if (plan == null || await HasActiveMemberShipsAsync(planID, cancellationToken))
        {
            return false;
        }

        // IsActive property removed from Plan entity
        plan.UpdatedAt = DateTime.Now;

        _unitOfWork.GetRepository<Plan>().Update(plan);
        return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
    }

    #endregion

    #region Get All Plans

    public async Task<IEnumerable<PlanViewModel>> GetAllPlansAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _unitOfWork.GetRepository<Plan>().GetAllAsync(null, cancellationToken);
        var plansList = plans.ToList();

        if (!plansList.Any())
        {
            return [];
        }

        return plansList.Select(p => new PlanViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            DurationDays = p.DurationDays,
            Price = p.Price,
            IsActive = true
        });
    }

    #endregion

    #region Get Plan By Id

    public async Task<PlanViewModel?> GetPlanByIdAsync(int planID, CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.GetRepository<Plan>().GetByIDAsync(planID, cancellationToken);
        if (plan == null)
        {
            return null;
        }

        return new PlanViewModel
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            DurationDays = plan.DurationDays,
            Price = plan.Price,
            IsActive = true
        };
    }

    #endregion

    #region Get Plan For Update

    public async Task<UpdatePlanViewModel?> GetPlanToUpdateAsync(int planID, CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.GetRepository<Plan>().GetByIDAsync(planID, cancellationToken);
        if (plan == null)
        {
            return null;
        }

        return new UpdatePlanViewModel
        {
            PlanName = plan.Name,
            Description = plan.Description,
            DurationDays = plan.DurationDays,
            Price = plan.Price
        };
    }

    #endregion

    #region Update Plan

    public async Task<bool> UpdatePlanAsync(int planID, UpdatePlanViewModel viewModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIDAsync(planID, cancellationToken);
            if (plan == null || await HasActiveMemberShipsAsync(planID, cancellationToken))
            {
                return false;
            }

            plan.Description = viewModel.Description;
            plan.DurationDays = viewModel.DurationDays;
            plan.Price = viewModel.Price;
            plan.Name = viewModel.PlanName;
            plan.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Plan>().Update(plan);
            return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    #region Helper Methods

    private async Task<bool> HasActiveMemberShipsAsync(int planID, CancellationToken cancellationToken = default)
    {
        var memberships = await _unitOfWork.GetRepository<MemberShip>().GetAllAsync(
            ms => ms.PlanId == planID && ms.EndDate >= DateTime.Now, cancellationToken);
        return memberships.Any();
    }

    #endregion
}
