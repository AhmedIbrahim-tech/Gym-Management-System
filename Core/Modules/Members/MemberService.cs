using Infrastructure.Entities.Membership;
using Infrastructure.Entities.Sessions;
using Infrastructure.Entities.Users.GymUsers;
using Infrastructure.Entities.Users;
using Infrastructure.Entities.Shared;

namespace Core.Modules.Members;

public interface IMemberService
{
    Task<bool> CreateMemberAsync(CreateMemberViewModel memberViewModel, CancellationToken cancellationToken = default);
    Task<bool> UpdateMemberAsync(int id, MemberToUpdateViewModel memberViewModel, CancellationToken cancellationToken = default);
    Task<bool> RemoveMemberAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ToggleMemberStatusAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MemberViewModel>> GetAllMembersAsync(string? currentUserId = null, CancellationToken cancellationToken = default);
    Task<MemberViewModel?> GetMemberDetailsAsync(int id, string? currentUserId = null, CancellationToken cancellationToken = default);
    Task<HealthRecordViewModel?> GetMemberHealthRecordAsync(int id, CancellationToken cancellationToken = default);
    Task<MemberToUpdateViewModel?> GetMemberToUpdateAsync(int id, CancellationToken cancellationToken = default);
}

public class MemberService(
    IUnitOfWork _unitOfWork, 
    IAttachmentService _attachmentService,
    UserManager<ApplicationUser> _userManager) : IMemberService
{
    #region Create Member

    public async Task<bool> CreateMemberAsync(CreateMemberViewModel memberViewModel, CancellationToken cancellationToken = default)
    {
        try
        {
            if (memberViewModel.FormFile == null || memberViewModel.FormFile.Length == 0)
            {
                return false;
            }

            if (await IsEmailExistAsync(memberViewModel.Email, cancellationToken) ||
                await IsPhoneExistAsync(memberViewModel.Phone, cancellationToken))
            {
                return false;
            }

            var photoName = _attachmentService.Upload("members", memberViewModel.FormFile);
            if (string.IsNullOrEmpty(photoName))
            {
                return false;
            }

            var member = new Member
            {
                Name = memberViewModel.Name,
                Email = memberViewModel.Email,
                DateOfBirth = memberViewModel.DateOfBirth,
                Phone = memberViewModel.Phone,
                Address = new Address
                {
                    Street = memberViewModel.Street,
                    City = memberViewModel.City,
                    BuildingNumber = memberViewModel.BuildingNumber
                },
                Gender = memberViewModel.Gender,
                PhotoUrl = photoName
            };

            await _unitOfWork.GetRepository<Member>().AddAsync(member, cancellationToken);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                _attachmentService.Delete(member.PhotoUrl, "members");
                return false;
            }

            var healthRecord = new HealthRecord
            {
                MemberId = member.Id,
                Height = memberViewModel.HealthRecordViewModel.Height,
                Weight = memberViewModel.HealthRecordViewModel.Weight,
                BloodType = memberViewModel.HealthRecordViewModel.BloodType,
                Note = memberViewModel.HealthRecordViewModel.Note
            };

            await _unitOfWork.GetRepository<HealthRecord>().AddAsync(healthRecord, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    #region Get All Members

    public async Task<IEnumerable<MemberViewModel>> GetAllMembersAsync(string? currentUserId = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<Member> members;
        
        // Filter based on user role
        if (!string.IsNullOrEmpty(currentUserId))
        {
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser != null)
            {
                var userRoles = await _userManager.GetRolesAsync(currentUser);
                var isSuperAdminOrAdmin = userRoles.Contains(Roles.SuperAdmin) || userRoles.Contains(Roles.Admin);
                
                if (isSuperAdminOrAdmin)
                {
                    // SuperAdmin and Admin see all members
                    members = await _unitOfWork.GetRepository<Member>().GetAllAsync(null, cancellationToken);
                }
                else if (userRoles.Contains(Roles.Member) && currentUser.MemberId.HasValue)
                {
                    // Members only see their own data
                    members = await _unitOfWork.GetRepository<Member>().GetAllAsync(
                        m => m.Id == currentUser.MemberId.Value, cancellationToken);
                }
                else if (userRoles.Contains(Roles.Trainer) && currentUser.TrainerId.HasValue)
                {
                    // Trainers see members who have bookings in their sessions
                    var trainerSessions = await _unitOfWork.GetRepository<Session>().GetAllAsync(
                        s => s.TrainerId == currentUser.TrainerId.Value, cancellationToken);
                    var sessionIds = trainerSessions.Select(s => s.Id).ToList();
                    
                    var bookings = await _unitOfWork.GetRepository<Booking>().GetAllAsync(
                        b => sessionIds.Contains(b.SessionId), cancellationToken);
                    var memberIds = bookings.Select(b => b.MemberId).Distinct().ToList();
                    
                    members = await _unitOfWork.GetRepository<Member>().GetAllAsync(
                        m => memberIds.Contains(m.Id), cancellationToken);
                }
                else
                {
                    // No access
                    return [];
                }
            }
            else
            {
                return [];
            }
        }
        else
        {
            // If no user ID provided, return all (for backward compatibility)
            members = await _unitOfWork.GetRepository<Member>().GetAllAsync(null, cancellationToken);
        }
        
        var membersList = members.ToList();

        if (!membersList.Any())
        {
            return [];
        }

        var memberViewModels = new List<MemberViewModel>();

        foreach (var member in membersList)
        {
            var memberViewModel = new MemberViewModel
            {
                Id = member.Id,
                Name = member.Name,
                Email = member.Email,
                DateOfBirth = member.DateOfBirth.ToShortDateString(),
                Phone = member.Phone,
                Photo = member.PhotoUrl,
                Gender = member.Gender.ToString()
            };

            // Get active membership for this member
            var memberships = await _unitOfWork.GetRepository<MemberShip>().GetAllAsync(
                ms => ms.MemberId == member.Id && ms.EndDate >= DateTime.Now, cancellationToken);
            var activeMembership = memberships.FirstOrDefault();

            if (activeMembership is not null)
            {
                var plan = await _unitOfWork.GetRepository<Plan>().GetByIDAsync(activeMembership.PlanId, cancellationToken);
                if (plan is not null)
                {
                    memberViewModel.PlanName = plan.Name;
                    memberViewModel.MembershipStartDate = activeMembership.CreatedAt.ToShortDateString();
                    memberViewModel.MembershipEndDate = activeMembership.EndDate.ToShortDateString();
                }
            }

            memberViewModels.Add(memberViewModel);
        }

        return memberViewModels;
    }

    #endregion

    #region Get Member Details

    public async Task<MemberViewModel?> GetMemberDetailsAsync(int id, string? currentUserId = null, CancellationToken cancellationToken = default)
    {
        var member = await _unitOfWork.GetRepository<Member>().GetByIDAsync(id, cancellationToken);
        if (member is null)
        {
            return null;
        }

        // Check if current user has access to this member
        if (!string.IsNullOrEmpty(currentUserId))
        {
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser != null)
            {
                var userRoles = await _userManager.GetRolesAsync(currentUser);
                var isSuperAdminOrAdmin = userRoles.Contains(Roles.SuperAdmin) || userRoles.Contains(Roles.Admin);
                
                if (!isSuperAdminOrAdmin)
                {
                    if (userRoles.Contains(Roles.Member) && currentUser.MemberId.HasValue)
                    {
                        // Members can only see their own data
                        if (member.Id != currentUser.MemberId.Value)
                        {
                            return null; // Access denied
                        }
                    }
                    else if (userRoles.Contains(Roles.Trainer) && currentUser.TrainerId.HasValue)
                    {
                        // Trainers can only see members who have bookings in their sessions
                        var trainerSessions = await _unitOfWork.GetRepository<Session>().GetAllAsync(
                            s => s.TrainerId == currentUser.TrainerId.Value, cancellationToken);
                        var sessionIds = trainerSessions.Select(s => s.Id).ToList();
                        
                        var hasBooking = await _unitOfWork.GetRepository<Booking>().GetAllAsync(
                            b => sessionIds.Contains(b.SessionId) && b.MemberId == member.Id, cancellationToken);
                        
                        if (!hasBooking.Any())
                        {
                            return null; // Access denied
                        }
                    }
                    else
                    {
                        return null; // Access denied
                    }
                }
            }
        }

        var memberViewModel = new MemberViewModel
        {
            Id = member.Id,
            Name = member.Name,
            Email = member.Email,
            DateOfBirth = member.DateOfBirth.ToShortDateString(),
            Phone = member.Phone,
            Photo = member.PhotoUrl,
            Gender = member.Gender.ToString(),
            Address = FormatAddress(member.Address)
        };

        var memberships = await _unitOfWork.GetRepository<MemberShip>().GetAllAsync(
            ms => ms.MemberId == id && ms.EndDate >= DateTime.Now, cancellationToken);
        var activeMembership = memberships.FirstOrDefault();

        if (activeMembership is not null)
        {
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIDAsync(activeMembership.PlanId, cancellationToken);
            if (plan is not null)
            {
                memberViewModel.PlanName = plan.Name;
                memberViewModel.MembershipStartDate = activeMembership.CreatedAt.ToShortDateString();
                memberViewModel.MembershipEndDate = activeMembership.EndDate.ToShortDateString();
            }
        }

        // Load health record
        var healthRecord = await GetMemberHealthRecordAsync(id, cancellationToken);
        memberViewModel.HealthRecordViewModel = healthRecord;

        return memberViewModel;
    }

    #endregion

    #region Update Member

    public async Task<bool> UpdateMemberAsync(int id, MemberToUpdateViewModel memberViewModel, CancellationToken cancellationToken = default)
    {
        var member = await _unitOfWork.GetRepository<Member>().GetByIDAsync(id, cancellationToken);
        if (member is null)
        {
            return false;
        }

        var membersByEmail = await _unitOfWork.GetRepository<Member>().GetAllAsync(
            m => m.Email.ToLower() == memberViewModel.Email.ToLower() && m.Id != id, cancellationToken);
        var emailExists = membersByEmail.Any();

        var membersByPhone = await _unitOfWork.GetRepository<Member>().GetAllAsync(
            m => m.Phone == memberViewModel.Phone && m.Id != id, cancellationToken);
        var phoneExists = membersByPhone.Any();

        if (emailExists || phoneExists)
        {
            return false;
        }

        if (memberViewModel.PhotoFile != null && memberViewModel.PhotoFile.Length > 0)
        {
            var uploadedPhoto = _attachmentService.Upload("members", memberViewModel.PhotoFile);

            if (uploadedPhoto != null)
            {
                if (!string.IsNullOrEmpty(member.PhotoUrl))
                {
                    _attachmentService.Delete(member.PhotoUrl, "members");
                }

                member.PhotoUrl = uploadedPhoto;
            }
            else
            {
                Console.WriteLine("Photo upload failed, skipping deletion.");
                return false;
            }
        }

        member.Email = memberViewModel.Email;
        member.Phone = memberViewModel.Phone;

        if (member.Address is null)
        {
            member.Address = new Address();
        }
        member.Address.Street = memberViewModel.Street;
        member.Address.City = memberViewModel.City;
        member.Address.BuildingNumber = memberViewModel.BuildingNumber;
        member.UpdatedAt = DateTime.Now;

        _unitOfWork.GetRepository<Member>().Update(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update health record if provided
        if (memberViewModel.HealthRecordViewModel != null)
        {
            var healthRecords = await _unitOfWork.GetRepository<HealthRecord>().GetAllAsync(
                hr => hr.MemberId == id, cancellationToken);
            var healthRecord = healthRecords.FirstOrDefault();

            if (healthRecord != null)
            {
                healthRecord.Height = memberViewModel.HealthRecordViewModel.Height;
                healthRecord.Weight = memberViewModel.HealthRecordViewModel.Weight;
                healthRecord.BloodType = memberViewModel.HealthRecordViewModel.BloodType;
                healthRecord.Note = memberViewModel.HealthRecordViewModel.Note;
                healthRecord.UpdatedAt = DateTime.Now;
                _unitOfWork.GetRepository<HealthRecord>().Update(healthRecord);
            }
            else
            {
                // Create new health record if it doesn't exist
                var newHealthRecord = new HealthRecord
                {
                    MemberId = id,
                    Height = memberViewModel.HealthRecordViewModel.Height,
                    Weight = memberViewModel.HealthRecordViewModel.Weight,
                    BloodType = memberViewModel.HealthRecordViewModel.BloodType,
                    Note = memberViewModel.HealthRecordViewModel.Note
                };
                await _unitOfWork.GetRepository<HealthRecord>().AddAsync(newHealthRecord, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    #endregion

    #region Get Member Health Record

    public async Task<HealthRecordViewModel?> GetMemberHealthRecordAsync(int id, CancellationToken cancellationToken = default)
    {
        var healthRecords = await _unitOfWork.GetRepository<HealthRecord>().GetAllAsync(
            hr => hr.MemberId == id, cancellationToken);
        var healthRecord = healthRecords.FirstOrDefault();
        
        if (healthRecord is null)
        {
            return null;
        }

        return new HealthRecordViewModel
        {
            Height = healthRecord.Height,
            Weight = healthRecord.Weight,
            BloodType = healthRecord.BloodType,
            Note = healthRecord.Note
        };
    }

    #endregion

    #region Get Member For Update

    public async Task<MemberToUpdateViewModel?> GetMemberToUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        var member = await _unitOfWork.GetRepository<Member>().GetByIDAsync(id, cancellationToken);
        if (member is null)
        {
            return null;
        }

        var viewModel = new MemberToUpdateViewModel
        {
            Name = member.Name,
            Email = member.Email,
            Photo = member.PhotoUrl,
            Phone = member.Phone,
            BuildingNumber = member.Address?.BuildingNumber ?? string.Empty,
            Street = member.Address?.Street ?? string.Empty,
            City = member.Address?.City ?? string.Empty
        };

        // Load health record
        var healthRecord = await GetMemberHealthRecordAsync(id, cancellationToken);
        viewModel.HealthRecordViewModel = healthRecord;

        return viewModel;
    }

    #endregion

    #region Remove Member

    public async Task<bool> RemoveMemberAsync(int memberId, CancellationToken cancellationToken = default)
    {
        var member = await _unitOfWork.GetRepository<Member>().GetByIDAsync(memberId, cancellationToken);
        if (member is null)
        {
            return false;
        }

        var bookings = await _unitOfWork.GetRepository<Booking>().GetAllAsync(
            b => b.MemberId == memberId, cancellationToken);
        var sessionIds = bookings.Select(b => b.SessionId).ToList();

        var sessions = await _unitOfWork.GetRepository<Session>().GetAllAsync(
            s => sessionIds.Contains(s.Id) && s.StartDate > DateTime.Now, cancellationToken);
        var hasFutureSessions = sessions.Any();

        if (hasFutureSessions)
        {
            return false;
        }

        var memberships = await _unitOfWork.GetRepository<MemberShip>().GetAllAsync(
            x => x.MemberId == memberId, cancellationToken);
        var membershipsList = memberships.ToList();

        try
        {
            if (membershipsList.Any())
            {
                foreach (var membership in membershipsList)
                {
                    _unitOfWork.GetRepository<MemberShip>().Delete(membership);
                }
            }

            _unitOfWork.GetRepository<Member>().Delete(member);
            var isDeleted = await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;

            if (isDeleted && !string.IsNullOrEmpty(member.PhotoUrl))
            {
                _attachmentService.Delete(member.PhotoUrl, "members");
            }

            return isDeleted;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Toggle Member Status

    public async Task<bool> ToggleMemberStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var memberships = await _unitOfWork.GetRepository<MemberShip>().GetAllAsync(
                ms => ms.MemberId == id, cancellationToken);
            var activeMembership = memberships.FirstOrDefault(ms => ms.EndDate >= DateTime.Now && ms.IsActive);

            if (activeMembership != null)
            {
                // Deactivate: Set IsActive to false and set EndDate to now
                activeMembership.IsActive = false;
                activeMembership.EndDate = DateTime.Now;
                activeMembership.UpdatedAt = DateTime.Now;
                _unitOfWork.GetRepository<MemberShip>().Update(activeMembership);
            }
            else
            {
                // Activate: Find the most recent inactive membership and reactivate it, or extend end date
                var inactiveMembership = memberships.OrderByDescending(ms => ms.CreatedAt).FirstOrDefault();
                if (inactiveMembership != null)
                {
                    inactiveMembership.IsActive = true;
                    inactiveMembership.EndDate = DateTime.Now.AddMonths(1); // Extend by 1 month
                    inactiveMembership.UpdatedAt = DateTime.Now;
                    _unitOfWork.GetRepository<MemberShip>().Update(inactiveMembership);
                }
                else
                {
                    // No membership exists, cannot activate
                    return false;
                }
            }

            return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Helper Methods

    private string FormatAddress(Address? address)
    {
        if (address is null)
        {
            return "N/A";
        }
        return $"{address.Street}, {address.BuildingNumber}, {address.City}";
    }

    private async Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken = default)
    {
        var members = await _unitOfWork.GetRepository<Member>().GetAllAsync(
            m => m.Email.ToLower() == email.ToLower(), cancellationToken);
        return members.Any();
    }

    private async Task<bool> IsPhoneExistAsync(string phone, CancellationToken cancellationToken = default)
    {
        var members = await _unitOfWork.GetRepository<Member>().GetAllAsync(
            m => m.Phone == phone, cancellationToken);
        return members.Any();
    }

    #endregion
}
