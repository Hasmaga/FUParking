﻿using FUParkingModel.DatabaseContext;
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

        public async Task<Return<IEnumerable<Transaction>>> GetWalletTransactionByWalletIdAsync(Guid walletId, int pageIndex, int pageSize, DateTime fromDate, DateTime toDate)
        {
            Return<IEnumerable<Transaction>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                // get wallet by id from and to date
                IEnumerable<Transaction> transactions = await _db.Transactions.Where(t => t.WalletId.Equals(walletId) && t.CreatedDate >= toDate && t.CreatedDate <= fromDate && t.DeletedDate == null)
                                                                .OrderByDescending(t => t.CreatedDate)
                                                                .Skip((pageIndex - 1) * pageSize)
                                                                .Take(pageSize)
                                                                .ToListAsync();
                res.Data = transactions;
                res.TotalRecord = transactions.Count();
                res.Message = transactions.Any() ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT;
                res.IsSuccess = true;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
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
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<Wallet>> GetMainWalletByCustomerId(Guid customerId)
        {
            try
            {
                var result = await _db.Wallets.FirstOrDefaultAsync(w => w.CustomerId.Equals(customerId) && w.WalletType == WalletType.MAIN);
                return new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = result,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<Wallet>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<Wallet>> GetExtraWalletByCustomerId(Guid customerId)
        {
            try
            {
                var result = await _db.Wallets
                    .FirstOrDefaultAsync(w => w.CustomerId
                    .Equals(customerId) && w.WalletType == WalletType.EXTRA);
                return new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = result,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<Wallet>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<Wallet>> UpdateWalletAsync(Wallet wallet)
        {
            try
            {
                _db.Wallets.Update(wallet);
                await _db.SaveChangesAsync();
                return new Return<Wallet>
                {
                    Data = wallet,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Wallet>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> UpdateAllWalletExtraAndMinusBalanceBackgroundTaskAsync()
        {
            try
            {
                var datetimenow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                // Get all ExpiredDate of Wallet Extra, if ExpiredDate <= TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")), set Extra = 0
                var wallets = await _db.Wallets.Where(w => w.WalletType == WalletType.EXTRA && w.EXPDate <= datetimenow).ToListAsync();
                foreach (var wallet in wallets)
                {
                    wallet.Balance = 0;
                    _db.Wallets.Update(wallet);
                    // Create transaction
                    FUParkingModel.Object.Transaction transaction = new()
                    {
                        WalletId = wallet.Id,
                        Amount = wallet.Balance,
                        TransactionStatus = StatusTransactionEnum.SUCCEED,
                        CreatedDate = datetimenow,
                        TransactionDescription = "The currency has expired.",
                    };
                    await _db.Transactions.AddAsync(transaction);
                    await _db.SaveChangesAsync();
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<dynamic>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
