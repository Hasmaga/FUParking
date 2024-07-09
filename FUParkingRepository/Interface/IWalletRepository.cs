using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IWalletRepository
    {
        public Task<Return<Wallet>> CreateWalletAsync(Wallet wallet);        
        public Task<Return<Wallet>> GetMainWalletByCustomerId(Guid customerId);
        public Task<Return<Wallet>> GetExtraWalletByCustomerId(Guid customerId);
        public Task<Return<IEnumerable<Transaction>>> GetWalletTransactionByWalletIdAsync(Guid walletId, int pageIndex, int pageSize, DateTime fromDate, DateTime toDate);
        public Task<Return<Wallet>> UpdateWalletAsync(Wallet wallet);
    }
}
