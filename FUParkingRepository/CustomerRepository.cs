using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public CustomerRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Customer>> CreateNewCustomerAsync(Customer customer)
        {
            try
            {
                await _db.Customers.AddAsync(customer);
                await _db.SaveChangesAsync();
                return new Return<Customer>
                {
                    Data = customer,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<Customer>
                {
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<IEnumerable<Customer>>> GetAllCustomerWithFilterAsync(Guid? CustomerTypeId = null, string? StatusCustomer = null)
        {
            try
            {
                var customers = await _db.Customers
                    .Where(c => (CustomerTypeId == null || c.CustomerTypeId == CustomerTypeId) && (StatusCustomer == null || c.StatusCustomer == StatusCustomer))
                    .ToListAsync();
                return new Return<IEnumerable<Customer>>
                {
                    Data = customers,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<Customer>>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<Customer>> GetCustomerByEmail(string email)
        {
            try
            {
                return new Return<Customer>
                {
                    Data = await _db.Customers.FirstOrDefaultAsync(c => c.Email == email),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<Customer>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<Customer>> GetCustomerByIdAsync(Guid customerId)
        {
            try
            {
                return new Return<Customer>
                {
                    Data = await _db.Customers.Include(c => c.CustomerType).FirstOrDefaultAsync(c => c.Id.Equals(customerId)),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<Customer>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<Customer>> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                _db.Customers.Update(customer);
                await _db.SaveChangesAsync();
                return new Return<Customer>
                {
                    Data = customer,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<Customer>
                {
                    Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<CustomerType>> CreateCustomerTypeAsync(CustomerType customerType)
        {
            try
            {
                await _db.CustomerTypes.AddAsync(customerType);
                await _db.SaveChangesAsync();
                return new Return<CustomerType>
                {
                    Data = customerType,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<CustomerType>
                {
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<IEnumerable<CustomerType>>> GetAllCustomerTypeAsync()
        {
            try
            {
                return new Return<IEnumerable<CustomerType>>
                {
                    Data = await _db.CustomerTypes.ToListAsync(),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<CustomerType>>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<CustomerType>> GetCustomerTypeByNameAsync(string name)
        {
            try
            {
                var customerType = await _db.CustomerTypes.FirstOrDefaultAsync(ct => ct.Name == name);
                return new Return<CustomerType>
                {
                    Data = customerType,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<CustomerType>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }
    }
}
