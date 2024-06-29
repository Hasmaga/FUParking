using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Customer;
using FUParkingModel.ResponseObject.Customer;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface ICustomerService
    {
        Task<Return<dynamic>> ChangeStatusCustomerAsync(ChangeStatusCustomerReqDto req);        
        Task<Return<GetCustomersWithFillerResDto>> GetCustomerByIdAsync(Guid customerId);
        Task<Return<IEnumerable<GetCustomersWithFillerResDto>>> GetListCustomerAsync(GetCustomersWithFillerReqDto req);
        Task<Return<dynamic>> CreateCustomerAsync(CustomerReqDto customer);
    }
}
