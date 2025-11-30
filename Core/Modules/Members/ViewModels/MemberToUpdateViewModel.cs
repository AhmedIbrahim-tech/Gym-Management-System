namespace Core.Modules.Members.ViewModels;

public class MemberToUpdateViewModel
{
    public string Name { get; set; } = null!;
    public string? Photo { get; set; }
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string BuildingNumber { get; set; } = null!;
    public HealthRecordViewModel? HealthRecordViewModel { get; set; }
    public IFormFile? PhotoFile { get; set; }
}
