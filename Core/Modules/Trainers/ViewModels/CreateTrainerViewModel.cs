using Microsoft.AspNetCore.Http;

namespace Core.Modules.Trainers.ViewModels;

public class CreateTrainerViewModel
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string BuildingNumber { get; set; } = null!;
    public Specialities? Specialization { get; set; }
    public IFormFile? PhotoFile { get; set; }
}
