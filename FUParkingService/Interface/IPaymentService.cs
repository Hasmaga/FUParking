using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPaymentService
    {
        Task<Return<IEnumerable<PaymentMethod>>> GetAllPaymentMethodAsync();
    }
}
