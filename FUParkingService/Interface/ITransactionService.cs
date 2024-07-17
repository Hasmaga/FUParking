using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.Transaction;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface ITransactionService
    {
        //Task<Return<List<Transaction>>> GetTransactionsAsync(DateTime fromDate, DateTime toDate, int pageSize, int pageIndex, Guid userGuid);
        Task<Return<IEnumerable<GetTransactionPaymentResDto>>> GetListTransactionPaymentAsync(GetListObjectWithFillerAttributeAndDateReqDto req);
    }
}
