namespace Core.Modules.Members.ViewModels;

public class HealthRecordViewModel
{
    public decimal Height { get; set; }
    public decimal Weight { get; set; }
    public string BloodType { get; set; } = null!;
    public string? Note { get; set; }
}
