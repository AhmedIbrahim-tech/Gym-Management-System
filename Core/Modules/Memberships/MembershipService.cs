using Core.Mappers;
using Infrastructure.Entities.Membership;
using Infrastructure.Entities.Users.GymUsers;

namespace Core.Modules.Memberships;

public interface IMembershipService
{
	Task<IEnumerable<MemberShipViewModel>> GetAllMemberShipsAsync(CancellationToken cancellationToken = default);
	Task<bool> CreateMembershipAsync(CreateMembershipViewModel createdMemberShip, CancellationToken cancellationToken = default);
	Task<bool> DeleteMemberShipAsync(int membershipId, CancellationToken cancellationToken = default);
	Task<IEnumerable<PlanSelectListViewModel>> GetPlansForDropDownAsync(CancellationToken cancellationToken = default);
	Task<IEnumerable<MemberSelectListViewModel>> GetMembersForDropDownAsync(CancellationToken cancellationToken = default);
}

public class MembershipService(IUnitOfWork _unitOfWork, IMembershipRepository _membershipRepository) : IMembershipService
{
    #region Create Membership

    public async Task<bool> CreateMembershipAsync(CreateMembershipViewModel createdMemberShip, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate member exists
            if (!await IsMemberExistsAsync(createdMemberShip.MemberId, cancellationToken))
            {
                return false;
            }

            // Validate plan exists
            if (!await IsPlanExistsAsync(createdMemberShip.PlanId, cancellationToken))
            {
                return false;
            }

            // Check for active membership
            if (await HasActiveMemberShipAsync(createdMemberShip.MemberId, cancellationToken))
            {
                return false;
            }

            // Get plan to calculate end date
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIDAsync(createdMemberShip.PlanId, cancellationToken);
            if (plan == null)
            {
                return false;
            }

            // Create membership
            var memberShipToCreate = createdMemberShip.ToMemberShip();
            var startDate = memberShipToCreate.StartDate;
            memberShipToCreate.EndDate = startDate.AddDays(plan.DurationDays);
            memberShipToCreate.IsActive = true;

            await _unitOfWork.GetRepository<MemberShip>().AddAsync(memberShipToCreate, cancellationToken);
            return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception)
        {
            // Log exception in production
            return false;
        }
    }

    #endregion

    #region Delete Membership

    public async Task<bool> DeleteMemberShipAsync(int membershipId, CancellationToken cancellationToken = default)
    {
        try
        {
            var membership = await _unitOfWork.GetRepository<MemberShip>().GetByIDAsync(membershipId, cancellationToken);
            
            if (membership == null)
            {
                return false;
            }

            // Soft delete: Mark as inactive instead of hard delete
            membership.IsActive = false;
            membership.UpdatedAt = DateTime.Now;
            
            _unitOfWork.GetRepository<MemberShip>().Update(membership);
            return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception)
        {
            // Log exception in production
            return false;
        }
    }

    #endregion

    #region Get All Memberships

    public async Task<IEnumerable<MemberShipViewModel>> GetAllMemberShipsAsync(CancellationToken cancellationToken = default)
    {
        var memberShips = await _membershipRepository.GetAllMembershipsWithMemberAndPlanAsync(
            x => x.IsActive && x.EndDate >= DateTime.Now, cancellationToken);
        
        return memberShips.Select(m => m.ToMemberShipViewModel());
    }

    #endregion

    #region Get Plans For Dropdown

    public async Task<IEnumerable<PlanSelectListViewModel>> GetPlansForDropDownAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _unitOfWork.GetRepository<Plan>().GetAllAsync(null, cancellationToken);
        return plans.Select(p => p.ToPlanSelectListViewModel());
    }

    #endregion

    #region Get Members For Dropdown

    public async Task<IEnumerable<MemberSelectListViewModel>> GetMembersForDropDownAsync(CancellationToken cancellationToken = default)
    {
        var members = await _unitOfWork.GetRepository<Member>().GetAllAsync(null, cancellationToken);
        return members.Select(m => m.ToMemberSelectListViewModel());
    }

    #endregion

    #region Helper Methods

    private async Task<bool> IsMemberExistsAsync(int memberId, CancellationToken cancellationToken = default)
    {
        var members = await _unitOfWork.GetRepository<Member>().GetAllAsync(x => x.Id == memberId, cancellationToken);
        return members.Any();
    }

    private async Task<bool> IsPlanExistsAsync(int planId, CancellationToken cancellationToken = default)
    {
        var plans = await _unitOfWork.GetRepository<Plan>().GetAllAsync(x => x.Id == planId, cancellationToken);
        return plans.Any();
    }

    private async Task<bool> HasActiveMemberShipAsync(int memberId, CancellationToken cancellationToken = default)
    {
        var memberships = await _unitOfWork.GetRepository<MemberShip>().GetAllAsync(
            x => x.MemberId == memberId && x.EndDate >= DateTime.Now, cancellationToken);
        return memberships.Any();
    }

    #endregion
}
