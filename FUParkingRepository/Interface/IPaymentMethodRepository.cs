using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IPaymentMethodRepository
    {
        Task<Return<PaymentMethod>> CreatePaymentMethodAsync (PaymentMethod paymentMethod);
        Task<Return<PaymentMethod>> UpdatePaymentMethodAsync (PaymentMethod paymentMethod);
        Task<Return<PaymentMethod>> GetPaymentMethodByIdAsync (Guid paymentMethodId);
        Task<Return<IEnumerable<PaymentMethod>>> GetAllPaymentMethodAsync();
    }
}
