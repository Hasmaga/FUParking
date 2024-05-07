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
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            } catch (Exception ex)
            {
                return new Return<Customer>
                {
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
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
                    SuccessfullyMessage = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            } catch (Exception ex)
            {
                return new Return<IEnumerable<Customer>>
                {
                    ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR,
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
                    Data = await _db.Customers.FindAsync(customerId),
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            } catch (Exception ex)
            {
                return new Return<Customer>
                {
                    ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR,
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
                    SuccessfullyMessage = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            } catch (Exception ex)
            {
                return new Return<Customer>
                {
                    ErrorMessage = ErrorEnumApplication.UPDATE_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }
    }
}
