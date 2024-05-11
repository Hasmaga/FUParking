using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public TransactionRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Transaction>> CreateTransactionAsync(Transaction transaction)
        {
            try
            {
                await _db.Transactions.AddAsync(transaction);
                await _db.SaveChangesAsync();
                return new Return<Transaction>
                {
                    Data = transaction,
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            } catch (Exception e)
            {
                return new Return<Transaction>
                {
                    IsSuccess = false,
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
