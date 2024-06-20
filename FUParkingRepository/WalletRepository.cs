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

        public async Task<Return<List<Transaction>>> GetWalletTransactionByWalletIdAsync(Guid walletId, int pageIndex, int pageSize, DateTime fromDate, DateTime toDate)
        {
            Return<List<Transaction>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                // get wallet by id from and to date
                List<Transaction> transactions = await _db.Transactions.Where(t => t.WalletId.Equals(walletId) && t.CreatedDate >= toDate && t.CreatedDate <= fromDate)
                                                                .OrderByDescending(t => t.CreatedDate)
                                                                .Skip((pageIndex - 1) * pageSize)
                                                                .Take(pageSize)
                                                                .ToListAsync();
                res.Data = transactions;
                res.Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY;
                res.IsSuccess = true;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex.Message;
                return res;
            }
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
                    res.Message = ErrorEnumApplication.WALLET_NOT_EXIST;
                    return res;
                }
                res.Data = wallet;
                res.IsSuccess = true;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex.Message;
                return res;
            }
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
                    InternalErrorMessage = e.Message,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
