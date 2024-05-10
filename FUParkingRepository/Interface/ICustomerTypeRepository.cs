using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface ICustomerTypeRepository
    {
        Task<Return<CustomerType>> GetCustomerTypeByNameAsync(string name);
    }
}
