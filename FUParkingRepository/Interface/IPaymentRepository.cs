using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IPaymentRepository
    {
        Task<Return<Payment>> CreatePaymentAsync(Payment payment);
    }
}
