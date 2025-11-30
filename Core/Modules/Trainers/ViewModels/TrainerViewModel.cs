namespace Core.Modules.Trainers.ViewModels;

public class TrainerViewModel
{
    public int? Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string DateOfBirth { get; set; } = null!;
    public string Gender { get; set; } = null!;
    public string? Address { get; set; } = null;
    public string specialization { get; set; } = null!;
    public string? Photo { get; set; }
}
