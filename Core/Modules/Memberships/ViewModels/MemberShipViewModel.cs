namespace Core.Modules.Memberships.ViewModels;

public class MemberShipViewModel
{
	public int Id { get; set; }
	public int MemberId { get; set; }
	public int PlanId { get; set; }
	public string MemberName { get; set; } = null!;
	public string PlanName { get; set; } = null!;
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public bool IsActive { get; set; }
	public int RemainingDays => (EndDate - DateTime.Now).Days > 0 ? (EndDate - DateTime.Now).Days : 0;
	public bool IsExpiringSoon => RemainingDays > 0 && RemainingDays <= 7;
}
