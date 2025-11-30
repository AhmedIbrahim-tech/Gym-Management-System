namespace Core.Modules.Plans.ViewModels;

public class UpdatePlanViewModel
{
	public string PlanName { get; set; } = null!;
	public string Description { get; set; } = null!;
	public int DurationDays { get; set; }
	public decimal Price { get; set; }
}
