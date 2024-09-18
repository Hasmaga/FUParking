using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Transaction;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ResponseObject.Transaction;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface ITransactionService
    {
        //Task<Return<List<Transaction>>> GetTransactionsAsync(DateTime fromDate, DateTime toDate, int pageSize, int pageIndex, Guid userGuid);
        Task<Return<IEnumerable<GetTransactionPaymentResDto>>> GetListTransactionPaymentAsync(GetListObjectWithFillerAttributeAndDateReqDto req);
        Task<Return<int>> GetRevenueTodayAsync();
        Task<Return<IEnumerable<StatisticParkingAreaRevenueResDto>>> GetListStatisticParkingAreaRevenueAsync();
        Task<Return<IEnumerable<StatisticRevenueParkingAreasDetailsResDto>>> GetListStatisticRevenueParkingAreasDetailsAsync(GetListObjectWithFillerDateAndSearchInputResDto req);
        Task<Return<dynamic>> TopUpCustomerWalletByUserAsync(TopUpCustomerWalletByUserReqDto req);
        Task<Return<IEnumerable<StatisticRevenueOfParkingSystemResDto>>> GetListStatisticRevenueOfParkingSystemAsync(DateTime? startDate, DateTime? endDate);
        Task<Return<IEnumerable<StatisticRevenueOfParkingAreaSystemDetailResDto>>> GetListStatisticRevenueOfParkingSystemDetailsAsync(Guid parkingId, DateTime? startDate, DateTime? endDate);
    }
}
