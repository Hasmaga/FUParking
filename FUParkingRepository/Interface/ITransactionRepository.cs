using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface ITransactionRepository
    {
        Task<Return<Transaction>> CreateTransactionAsync(Transaction transaction);
        Task<Return<List<Transaction>>> GetTransactionListAsync(DateTime FromDate, DateTime ToDate, int pageSize, int pageIndex);
    }
}
