using Core.Mappers;
using Infrastructure.Entities.Membership;
using Infrastructure.Entities.Users.GymUsers;

namespace Core.Modules.Payments;

public interface IPaymentService
{
    Task<bool> CreatePaymentAsync(CreatePaymentViewModel viewModel, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentViewModel>> GetAllPaymentsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentViewModel>> GetMemberPaymentsAsync(int memberId, CancellationToken cancellationToken = default);
    Task<PaymentViewModel?> GetPaymentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalPaymentsByMemberAsync(int memberId, CancellationToken cancellationToken = default);
}

public class PaymentService(IUnitOfWork _unitOfWork) : IPaymentService
{
    #region Create Payment

    public async Task<bool> CreatePaymentAsync(CreatePaymentViewModel viewModel, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if member exists
            var member = await _unitOfWork.GetRepository<Member>().GetByIDAsync(viewModel.MemberId, cancellationToken);
            if (member is null)
            {
                return false;
            }

            // Validate amount
            if (viewModel.Amount <= 0)
            {
                return false;
            }

            // Validate payment method
            var validMethods = new[] { "Cash", "Card", "Online", "Bank Transfer" };
            if (!validMethods.Contains(viewModel.Method, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }

            var payment = viewModel.ToPayment();
            await _unitOfWork.GetRepository<Payment>().AddAsync(payment, cancellationToken);
            return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Get All Payments

    public async Task<IEnumerable<PaymentViewModel>> GetAllPaymentsAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.GetRepository<Payment>().GetAllAsync(
            null, cancellationToken, p => p.Member);
        var paymentsList = payments.OrderByDescending(p => p.PaymentDate).ToList();

        if (!paymentsList.Any())
        {
            return [];
        }

        return paymentsList.Select(p => p.ToPaymentViewModel());
    }

    #endregion

    #region Get Member Payments

    public async Task<IEnumerable<PaymentViewModel>> GetMemberPaymentsAsync(int memberId, CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.GetRepository<Payment>().GetAllAsync(
            p => p.MemberId == memberId, cancellationToken, p => p.Member);
        var paymentsList = payments.OrderByDescending(p => p.PaymentDate).ToList();

        if (!paymentsList.Any())
        {
            return [];
        }

        return paymentsList.Select(p => p.ToPaymentViewModel());
    }

    #endregion

    #region Get Payment By Id

    public async Task<PaymentViewModel?> GetPaymentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.GetRepository<Payment>().GetByIDAsync(id, cancellationToken);
        if (payment is null)
        {
            return null;
        }

        return payment.ToPaymentViewModel();
    }

    #endregion

    #region Get Total Payments By Member

    public async Task<decimal> GetTotalPaymentsByMemberAsync(int memberId, CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.GetRepository<Payment>().GetAllAsync(
            p => p.MemberId == memberId, cancellationToken);
        return payments.Sum(p => p.Amount);
    }

    #endregion
}

