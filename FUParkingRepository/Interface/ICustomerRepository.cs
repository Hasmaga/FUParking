using FUParkingModel.Object;
using FUParkingModel.RequestObject.Customer;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface ICustomerRepository
    {
        Task<Return<Customer>> CreateNewCustomerAsync(Customer customer);
        Task<Return<Customer>> GetCustomerByIdAsync(Guid customerId);
        Task<Return<IEnumerable<Customer>>> GetAllCustomerWithFilterAsync(Guid? CustomerTypeId = null, string? StatusCustomer = null);
        Task<Return<Customer>> UpdateCustomerAsync(Customer customer);
        Task<Return<Customer>> GetCustomerByEmailAsync(string email);
        Task<Return<CustomerType>> GetCustomerTypeByNameAsync(string name);
        Task<Return<IEnumerable<CustomerType>>> GetAllCustomerTypeAsync();
        Task<Return<CustomerType>> CreateCustomerTypeAsync(CustomerType customerType);
        Task<Return<List<Customer>>> GetListCustomerAsync(GetCustomersWithFillerReqDto req);
    }
}
