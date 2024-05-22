using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IWalletService
    {
        public Task<Return<List<Transaction>>> GetWalletTransactionByCustomerIdAsync(string? customerId, int pageIndex, int pageSize, int numberOfDate);
    }
}
