using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPaymentMethodService
    {
        Task<Return<bool>> CreatePaymentMethodAsync(CreatePaymentMethodReqDto req);
        Task<Return<bool>> ChangeStatusPaymentMethodAsync(ChangeStatusPaymentMethodReqDto req);
    }
}
