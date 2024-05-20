using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    public class DepositRepository : IDepositRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public DepositRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Deposit?>> CreateDepositAsync(Deposit deposit)
        {
            try
            {
                await _db.Deposits.AddAsync(deposit);
                await _db.SaveChangesAsync();
                return new Return<Deposit?>
                {
                    Data = deposit,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Deposit?>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
