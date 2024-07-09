using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface ITransactionRepository
    {
        Task<Return<Transaction>> CreateTransactionAsync(Transaction transaction);
        Task<Return<IEnumerable<Transaction>>> GetTransactionListAsync(DateTime FromDate, DateTime ToDate, int pageSize, int pageIndex);
        Task<Return<Transaction>> GetTransactionByDepositIdAsync(Guid transactionId);
        Task<Return<Transaction>> UpdateTransactionAsync(Transaction transaction);
        Task<Return<IEnumerable<Transaction>>> GetTransByWalletIdAsync(Guid walletId, int pageSize, int pageIndex, DateTime? startDate, DateTime? endDate);
    }
}
