namespace Core.Modules.Attendances.ViewModels;

public class AttendanceViewModel
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = null!;
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string Duration { get; set; } = string.Empty;
}

