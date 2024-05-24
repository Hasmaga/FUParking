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
        Task<Return<List<Customer>>> GetListCustomerAsync(Guid userId, int pageSize, int pageIndex);
        Task<Return<Customer>> CreateCustomerAsync(CustomerReqDto customer, Guid userGuid);

        
    }
}
