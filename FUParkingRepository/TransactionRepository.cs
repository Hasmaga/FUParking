using FUParkingModel.DatabaseContext;
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

        public Task<Return<Transaction>> CreateTransactionAsync(Transaction transaction)
        {
            throw new NotImplementedException();
        }
    }
}
