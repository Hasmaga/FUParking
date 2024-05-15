using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface ICustomerService
    {
        Task<Return<bool>> ChangeStatusCustomerAsync(ChangeStatusCustomerReqDto req);
    }
}
