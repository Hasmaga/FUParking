using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface ICustomerService
    {
        Task<Return<bool>> ChangeStatusCustomerAsync(ChangeStatusCustomerReqDto req);
        Task<Return<bool>> BuyPackageAsync(BuyPackageReqDto req, Guid customerId);
        Task<Return<Customer>> GetCustomerByIdAsync(Guid customerId);

        
    }
}
