using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class WalletRepository : IWalletRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public WalletRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Wallet?>> GetWalletByCustomerId(Guid customerId)
        {
            Return<Wallet?> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                Wallet? wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.CustomerId.Equals(customerId));
                if (wallet == null)
                {
                    throw new KeyNotFoundException();
                }
                res.Data = wallet;

            }
            catch (Exception ex)
            {
                if (ex is KeyNotFoundException)
                {
                    res.Message = ErrorEnumApplication.WALLET_NOT_EXIST;
                }
            }
            return res;
        }

        public async Task<Return<Wallet>> CreateWalletAsync(Wallet wallet)
        {
            try
            {
                await _db.Wallets.AddAsync(wallet);
                await _db.SaveChangesAsync();
                return new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = wallet,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Wallet>
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR
                };
            }
        }
    }
}
