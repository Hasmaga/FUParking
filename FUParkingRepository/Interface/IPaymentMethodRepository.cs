using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IPaymentMethodRepository
    {
        Task<Return<PaymentMethod>> CreatePaymentMethodAsync(PaymentMethod paymentMethod);
    }
}
