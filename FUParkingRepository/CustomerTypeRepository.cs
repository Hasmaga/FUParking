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

        public async Task<Return<CustomerType>> GetCustomerTypeByNameAsync(string name)
        {
            try
            {
                var customerType = await _db.CustomerTypes.FirstOrDefaultAsync(ct => ct.Name == name);
                return new Return<CustomerType>
                {
                    Data = customerType,
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<CustomerType>
                {
                    ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }
    }
}
