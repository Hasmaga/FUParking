using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Customer;
using FUParkingModel.ResponseObject.Statistic;
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
                    IsSuccess = true,
                    Data = customer,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<Customer>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
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
                    TotalRecord = customers.Count,
                    Message = customers.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<Customer>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<Customer>> GetCustomerByEmailAsync(string email)
        {
            try
            {
                var customer = await _db.Customers.Where(p => p.DeletedDate == null).FirstOrDefaultAsync(c => c.Email == email);
                return new Return<Customer>
                {
                    Data = customer,
                    IsSuccess = true,
                    Message = customer != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<Customer>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
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
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<Customer>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
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
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
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
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<IEnumerable<CustomerType>>> GetAllCustomerTypeAsync()
        {
            try
            {
                var result = await _db.CustomerTypes.ToListAsync();
                return new Return<IEnumerable<CustomerType>>
                {
                    Data = result,
                    TotalRecord = result.Count,
                    IsSuccess = true,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<CustomerType>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<CustomerType>> GetCustomerTypeByNameAsync(string name)
        {
            try
            {
                var result = await _db.CustomerTypes.FirstOrDefaultAsync(ct => ct.Name == name);
                return new Return<CustomerType>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<CustomerType>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<IEnumerable<Customer>>> GetListCustomerAsync(GetCustomersWithFillerReqDto req)
        {
            Return<IEnumerable<Customer>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                var query = _db.Customers.Include(c => c.CustomerType).Where(c => c.DeletedDate == null).AsQueryable();
                if (!string.IsNullOrEmpty(req.Attribute) && !string.IsNullOrEmpty(req.SearchInput))
                {
                    switch (req.Attribute.ToLower())
                    {
                        case "fullname":
                            query = query.Where(c => c.FullName != null && c.FullName.Contains(req.SearchInput));
                            break;
                        case "email":
                            query = query.Where(c => c.Email != null && c.Email.Contains(req.SearchInput));
                            break;
                        case "customertype":
                            query = query.Where(c => c.CustomerType != null && c.CustomerType.Name != null && c.CustomerType.Name.Contains(req.SearchInput));
                            break;
                        case "statuscustomer":
                            query = query.Where(c => c.StatusCustomer != null && c.StatusCustomer.Contains(req.SearchInput));
                            break;
                        default:
                            break;
                    }
                }
                var result = await query
                    .OrderByDescending(t => t.CreatedDate)
                    .Skip((req.PageIndex - 1) * req.PageSize)
                    .Take(req.PageSize)
                    .ToListAsync();
                res.Data = result;
                res.TotalRecord = await query.CountAsync();
                res.Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT;
                res.IsSuccess = true;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
                return res;
            }
        }

        public async Task<Return<Customer>> GetCustomerByPlateNumberAsync(string plateNumber)
        {
            try
            {
#pragma warning disable CS8604 // Possible null reference argument.
                var query = await _db.Customers
                    .Include(c => c.CustomerType)
                    .Include(c => c.Vehicles)
                    .Where(c => c.Vehicles.Any(v => v.PlateNumber == plateNumber))
                    .FirstOrDefaultAsync();
#pragma warning restore CS8604 // Possible null reference argument.
                return new Return<Customer>
                {
                    Data = query,
                    IsSuccess = true,
                    Message = query != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<Customer>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<StatisticCustomerResDto>> StatisticCustomerAsync()
        {
            try
            {
                // Get total customer
                var totalCustomer = await _db.Customers.CountAsync();

                // Get total new customer in this month
                var totalNewCustomer = await _db.Customers
                    .Where(c => c.CreatedDate.Month == DateTime.Now.Month && c.CreatedDate.Year == DateTime.Now.Year)
                    .CountAsync();

                var result = new StatisticCustomerResDto
                {
                    TotalCustomer = totalCustomer,
                    TotalNewCustomerInMonth = totalNewCustomer
                };
                return new Return<StatisticCustomerResDto>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<StatisticCustomerResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }
    }
}
