using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IWalletRepository
    {
        public Task<Return<Wallet>> CreateWalletAsync(Wallet wallet);
        public Task<Return<Wallet?>> GetWalletByCustomerId(Guid customerId);
    }
}
