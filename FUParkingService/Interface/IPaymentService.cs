using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPaymentService
    {
        Task<Return<bool>> CreatePaymentMethodAsync(CreatePaymentMethodReqDto req);
        Task<Return<bool>> ChangeStatusPaymentMethodAsync(ChangeStatusPaymentMethodReqDto req);
        Task<Return<IEnumerable<PaymentMethod>>> GetAllPaymentMethodAsync();
    }
}
