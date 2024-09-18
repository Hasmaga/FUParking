using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.Statistic;
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
        Task<Return<IEnumerable<Transaction>>> GetListTransactionPaymentAsync(int pageSize, int pageIndex, DateTime? startDate, DateTime? endDate, string? searchInput = null, string? Attribute = null);
        Task<Return<int>> GetRevenueTodayAsync();
        Task<Return<IEnumerable<StatisticParkingAreaRevenueResDto>>> GetListStatisticParkingAreaRevenueAsync();
        Task<Return<Transaction>> GetTransactionBySessionIdAsync(Guid sessionId);
        Task<Return<IEnumerable<StatisticRevenueParkingAreasDetailsResDto>>> GetListStatisticRevenueParkingAreasDetailsAsync(GetListObjectWithFillerDateAndSearchInputResDto req);
        Task<Return<IEnumerable<StatisticRevenueOfParkingSystemResDto>>> GetListStatisticRevenueOfParkingSystemAsync(DateTime? startDate, DateTime? endDate);
        Task<Return<IEnumerable<StatisticRevenueOfParkingAreaSystemDetailResDto>>> GetListStatisticRevenueOfParkingSystemDetailsAsync(Guid parkingId, DateTime? startDate, DateTime? endDate);
    }
}
