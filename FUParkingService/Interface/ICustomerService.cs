using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Customer;
using FUParkingModel.ResponseObject.Customer;
using FUParkingModel.ResponseObject.Session;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface ICustomerService
    {
        Task<Return<dynamic>> ChangeStatusCustomerAsync(ChangeStatusCustomerReqDto req);
        Task<Return<GetCustomersWithFillerResDto>> GetCustomerByIdAsync(Guid customerId);
        Task<Return<IEnumerable<GetCustomersWithFillerResDto>>> GetListCustomerAsync(GetCustomersWithFillerReqDto req);
        Task<Return<dynamic>> CreateCustomerAsync(CustomerReqDto customer);
        Task<Return<StatisticCustomerResDto>> StatisticCustomerAsync();
        Task<Return<bool>> UpdateCustomerAccountAsync(UpdateCustomerAccountReqDto req);
        Task<Return<dynamic>> CreateNonPaidCustomerAsync(CreateNonPaidCustomerReqDto req);
        Task<Return<GetCustomerTypeByPlateNumberResDto>> GetCustomerTypeByPlateNumberAsync(string PlateNumber);
    }
}
