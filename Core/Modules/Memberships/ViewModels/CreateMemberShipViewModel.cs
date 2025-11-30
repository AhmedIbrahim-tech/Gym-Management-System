using System.ComponentModel.DataAnnotations;

namespace Core.Modules.Memberships.ViewModels;

public class CreateMembershipViewModel
{
	[Required(ErrorMessage = "Plan is required")]
	[Display(Name = "Plan")]
	public int PlanId { get; set; }

	[Required(ErrorMessage = "Member is required")]
	[Display(Name = "Member")]
	public int MemberId { get; set; }

	[Display(Name = "Start Date")]
	[DataType(DataType.Date)]
	public DateTime? StartDate { get; set; }

	public IEnumerable<PlanSelectListViewModel> Plans { get; set; } = [];
	public IEnumerable<MemberSelectListViewModel> Members { get; set; } = [];
}
