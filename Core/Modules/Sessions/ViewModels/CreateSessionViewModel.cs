namespace Core.Modules.Sessions.ViewModels;

public class CreateSessionViewModel
{
    public string Description { get; set; } = null!;
    public int Capacity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TrainerId { get; set; }
    public int CategoryId { get; set; }
}
