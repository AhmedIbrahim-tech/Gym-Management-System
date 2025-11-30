using Infrastructure.Entities.Sessions;
using Infrastructure.Entities.Users.GymUsers;
using Infrastructure.Entities.Shared;
using Core.ThirdParty.AttachmentService;

namespace Core.Modules.Trainers;

public interface ITrainerService
{
    Task<bool> CreateTrainerAsync(CreateTrainerViewModel trainerViewModel, CancellationToken cancellationToken = default);
    Task<bool> UpdateTrainerAsync(int id, TrainerToUpdateViewModel trainerViewModel, CancellationToken cancellationToken = default);
    Task<bool> RemoveTrainerAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrainerViewModel>> GetAllTrainersAsync(CancellationToken cancellationToken = default);
    Task<TrainerViewModel?> GetTrainerDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<TrainerToUpdateViewModel?> GetTrainerToUpdateAsync(int id, CancellationToken cancellationToken = default);
}

public class TrainerService(IUnitOfWork _unitOfWork, IAttachmentService _attachmentService) : ITrainerService
{
    #region Create Trainer

    public async Task<bool> CreateTrainerAsync(CreateTrainerViewModel trainerViewModel, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await IsEmailExistAsync(trainerViewModel.Email, cancellationToken) ||
                await IsPhoneExistAsync(trainerViewModel.Phone, cancellationToken))
            {
                return false;
            }

            string? photoUrl = null;
            if (trainerViewModel.PhotoFile != null && trainerViewModel.PhotoFile.Length > 0)
            {
                photoUrl = _attachmentService.Upload("trainers", trainerViewModel.PhotoFile);
                if (string.IsNullOrEmpty(photoUrl))
                {
                    return false;
                }
            }

            var trainer = new Trainer
            {
                Name = trainerViewModel.Name,
                Email = trainerViewModel.Email,
                DateOfBirth = trainerViewModel.DateOfBirth,
                Phone = trainerViewModel.Phone,
                Address = new Address
                {
                    Street = trainerViewModel.Street,
                    City = trainerViewModel.City,
                    BuildingNumber = trainerViewModel.BuildingNumber
                },
                Gender = trainerViewModel.Gender,
                Speciality = trainerViewModel.Specialization,
                PhotoUrl = photoUrl
            };

            await _unitOfWork.GetRepository<Trainer>().AddAsync(trainer, cancellationToken);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;

            if (!result && !string.IsNullOrEmpty(photoUrl))
            {
                _attachmentService.Delete(photoUrl, "trainers");
                return false;
            }

            return result;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    #region Get All Trainers

    public async Task<IEnumerable<TrainerViewModel>> GetAllTrainersAsync(CancellationToken cancellationToken = default)
    {
        var trainers = await _unitOfWork.GetRepository<Trainer>().GetAllAsync(null, cancellationToken);
        var trainersList = trainers.ToList();

        if (!trainersList.Any())
        {
            return [];
        }

        return trainersList.Select(t => new TrainerViewModel
        {
            Id = t.Id,
            Name = t.Name,
            Email = t.Email,
            DateOfBirth = t.DateOfBirth.ToShortDateString(),
            Phone = t.Phone,
            Gender = t.Gender.ToString(),
            specialization = t.Speciality?.ToString() ?? "N/A",
            Photo = t.PhotoUrl
        });
    }

    #endregion

    #region Get Trainer Details

    public async Task<TrainerViewModel?> GetTrainerDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIDAsync(id, cancellationToken);
        if (trainer is null)
        {
            return null;
        }

        return new TrainerViewModel
        {
            Id = trainer.Id,
            Name = trainer.Name,
            Email = trainer.Email,
            DateOfBirth = trainer.DateOfBirth.ToShortDateString(),
            Phone = trainer.Phone,
            Gender = trainer.Gender.ToString(),
            Address = FormatAddress(trainer.Address),
            specialization = trainer.Speciality?.ToString() ?? "N/A",
            Photo = trainer.PhotoUrl
        };
    }

    #endregion

    #region Update Trainer

    public async Task<bool> UpdateTrainerAsync(int id, TrainerToUpdateViewModel trainerViewModel, CancellationToken cancellationToken = default)
    {
        var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIDAsync(id, cancellationToken);
        if (trainer is null)
        {
            return false;
        }

        var trainers = await _unitOfWork.GetRepository<Trainer>().GetAllAsync(
            m => m.Email.ToLower() == trainerViewModel.Email.ToLower() && m.Id != id, cancellationToken);
        var emailExists = trainers.Any();

        var trainersByPhone = await _unitOfWork.GetRepository<Trainer>().GetAllAsync(
            m => m.Phone == trainerViewModel.Phone && m.Id != id, cancellationToken);
        var phoneExists = trainersByPhone.Any();

        if (emailExists || phoneExists)
        {
            return false;
        }

        if (trainerViewModel.PhotoFile != null && trainerViewModel.PhotoFile.Length > 0)
        {
            var uploadedPhoto = _attachmentService.Upload("trainers", trainerViewModel.PhotoFile);

            if (uploadedPhoto != null)
            {
                if (!string.IsNullOrEmpty(trainer.PhotoUrl))
                {
                    _attachmentService.Delete(trainer.PhotoUrl, "trainers");
                }

                trainer.PhotoUrl = uploadedPhoto;
            }
            else
            {
                return false;
            }
        }

        trainer.Name = trainerViewModel.Name;
        trainer.Email = trainerViewModel.Email;
        trainer.Phone = trainerViewModel.Phone;

        if (trainer.Address is null)
        {
            trainer.Address = new Address();
        }
        trainer.Address.Street = trainerViewModel.Street;
        trainer.Address.City = trainerViewModel.City;
        trainer.Address.BuildingNumber = trainerViewModel.BuildingNumber;
        trainer.UpdatedAt = DateTime.Now;
        trainer.Speciality = trainerViewModel.Specialization;

        _unitOfWork.GetRepository<Trainer>().Update(trainer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    #endregion

    #region Get Trainer For Update

    public async Task<TrainerToUpdateViewModel?> GetTrainerToUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIDAsync(id, cancellationToken);
        if (trainer is null)
        {
            return null;
        }

        return new TrainerToUpdateViewModel
        {
            Name = trainer.Name,
            Email = trainer.Email,
            Phone = trainer.Phone,
            Specialization = trainer.Speciality,
            BuildingNumber = trainer.Address?.BuildingNumber ?? string.Empty,
            Street = trainer.Address?.Street ?? string.Empty,
            City = trainer.Address?.City ?? string.Empty,
            DateOfBirth = trainer.DateOfBirth,
            Gender = trainer.Gender,
            CurrentPhotoUrl = trainer.PhotoUrl
        };
    }

    #endregion

    #region Remove Trainer

    public async Task<bool> RemoveTrainerAsync(int id, CancellationToken cancellationToken = default)
    {
        var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIDAsync(id, cancellationToken);
        if (trainer is null)
        {
            return false;
        }

        var activeBookings = await _unitOfWork.GetRepository<Session>().GetAllAsync(
            b => b.TrainerId == id && b.StartDate > DateTime.Now, cancellationToken);
        if (activeBookings.Any())
        {
            return false;
        }

        try
        {
            if (!string.IsNullOrEmpty(trainer.PhotoUrl))
            {
                _attachmentService.Delete(trainer.PhotoUrl, "trainers");
            }

            _unitOfWork.GetRepository<Trainer>().Delete(trainer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception)
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
        var trainers = await _unitOfWork.GetRepository<Trainer>().GetAllAsync(
            m => m.Email.ToLower() == email.ToLower(), cancellationToken);
        return trainers.Any();
    }

    private async Task<bool> IsPhoneExistAsync(string phone, CancellationToken cancellationToken = default)
    {
        var trainers = await _unitOfWork.GetRepository<Trainer>().GetAllAsync(
            m => m.Phone == phone, cancellationToken);
        return trainers.Any();
    }

    #endregion
}
