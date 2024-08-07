using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public TransactionRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Transaction>> CreateTransactionAsync(Transaction transaction)
        {
            try
            {
                await _db.Transactions.AddAsync(transaction);
                await _db.SaveChangesAsync();
                return new Return<Transaction>
                {
                    Data = transaction,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Transaction>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Transaction>> GetTransactionByDepositIdAsync(Guid transactionId)
        {
            try
            {
                var result = await _db.Transactions.Include(t => t.Payment).FirstOrDefaultAsync(t => t.DepositId.Equals(transactionId));
                return new Return<Transaction>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<Transaction>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<Transaction>>> GetTransactionListAsync(DateTime FromDate, DateTime ToDate, int pageSize, int pageIndex)
        {
            Return<IEnumerable<Transaction>> res = new() { Message = ErrorEnumApplication.SERVER_ERROR };
            try
            {
                var result = await _db.Transactions.Include(t => t.Payment).Where(t => t.CreatedDate >= FromDate && t.CreatedDate <= ToDate)
                                                                .OrderByDescending(t => t.CreatedDate)
                                                                .Skip((pageIndex - 1) * pageSize)
                                                                .Take(pageSize)
                                                                .ToListAsync();
                res.Data = result;
                res.Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT;
                res.IsSuccess = true;
                return res;
            }
            catch (Exception e)
            {
                res.InternalErrorMessage = e;
                return res;
            }
        }

        public async Task<Return<IEnumerable<Transaction>>> GetTransByWalletIdAsync(Guid walletId, int pageSize, int pageIndex, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                DateTime endDateValue = endDate ?? TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                DateTime startDateValue = startDate ?? DateTime.MinValue;

                var res = await _db.Transactions.Include(t => t.Payment)
                    .Where(t => t.WalletId.Equals(walletId) && t.CreatedDate >= startDateValue && t.CreatedDate <= endDateValue && t.TransactionStatus != StatusTransactionEnum.PENDING)
                    .OrderByDescending(t => t.CreatedDate)
                    .ToListAsync();

                var result = res
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize);

                return new Return<IEnumerable<Transaction>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = res.Count,
                    Message = res.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Transaction>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Transaction>> UpdateTransactionAsync(Transaction transaction)
        {
            try
            {
                _db.Update(transaction);
                await _db.SaveChangesAsync();
                return new Return<Transaction>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Transaction>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<Transaction>>> GetListTransactionPaymentAsync(int pageSize, int pageIndex, DateTime? startDate, DateTime? endDate, string? searchInput = null, string? Attribute = null)
        {
            try
            {
                DateTime endDateValue = endDate ?? TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                DateTime startDateValue = startDate ?? DateTime.MinValue;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var res = _db.Transactions
                    .Include(t => t.Payment)
                    .Include(t => t.Wallet)
                    .Include(t => t.Deposit)
                    .Include(t => t.Wallet.Customer)
                    .Include(t => t.Payment.PaymentMethod)
                    .Include(t => t.Payment.Session)
                    .Include(t => t.Payment.Session.GateIn)
                    .Include(t => t.Deposit.Package)                    
                    .AsQueryable();
                //.Where(t => t.CreatedDate >= startDateValue && t.CreatedDate <= endDateValue && t.PaymentId.HasValue)
                //.OrderByDescending(t => t.CreatedDate)
                //.ToListAsync();
                if (searchInput != null && Attribute != null)
                {
                    switch (Attribute.ToUpper())
                    {
                        case "EMAIL":
                            res = res.Where(t => t.Wallet.Customer.Email.Contains(searchInput));
                            break;
                        case "PACKAGENAME":
                            res = res.Where(t => t.Deposit.Package.Name.Contains(searchInput));
                            break;
                        case "TRANSACTIONSTATUS":
                            res = res.Where(t => t.TransactionStatus.Equals(searchInput));
                            break;
                        case "PAYMENTMETHOD":
                            res = res.Where(t => t.Payment.PaymentMethod.Equals(searchInput));
                            break;
                        default:
                            break;
                    }
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                var filter = await res
                    .Where(t => t.CreatedDate >= startDateValue && t.CreatedDate <= endDateValue && t.PaymentId.HasValue)
                    .OrderByDescending(t => t.CreatedDate)
                    .ToListAsync();

                var result = filter
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize);

                return new Return<IEnumerable<Transaction>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = filter.Count,
                    Message = filter.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Transaction>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }

        }
    }
}
