namespace Core.Modules.Sessions.ViewModels;

public class UpdateSessionViewModel
{
	public string Description { get; set; } = null!;
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public int TrainerId { get; set; }
}
