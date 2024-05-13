using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class CustomerTypeRepository : ICustomerTypeRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public CustomerTypeRepository(FUParkingDatabaseContext db)
        {
            _db = db;
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

        public async Task<Return<IEnumerable<CustomerType>>> GetAllCustomerType()
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
