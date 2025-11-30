namespace Core.Modules.Payments.ViewModels;

public class CreatePaymentViewModel
{
    public int MemberId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = null!;
}

