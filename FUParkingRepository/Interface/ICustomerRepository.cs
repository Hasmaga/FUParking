using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface ICustomerRepository
    {
        Task<Return<Customer>> CreateNewCustomerAsync(Customer customer);
        Task<Return<Customer>> GetCustomerByIdAsync(Guid customerId);
        Task<Return<IEnumerable<Customer>>> GetAllCustomerWithFilterAsync(Guid? CustomerTypeId = null, string? StatusCustomer = null);
        Task<Return<Customer>> UpdateCustomerAsync(Customer customer);
        Task<Return<Customer>> GetCustomerByEmail(string email);
    }
}
