using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPaymentService
    {
        Task<Return<dynamic>> CreatePaymentMethodAsync(CreatePaymentMethodReqDto req);
        Task<Return<dynamic>> ChangeStatusPaymentMethodAsync(ChangeStatusPaymentMethodReqDto req);
        Task<Return<IEnumerable<PaymentMethod>>> GetAllPaymentMethodAsync();
    }
}
