using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.ParkingArea;
using FUParkingModel.ResponseObject.Statistic;
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
                endDateValue = endDateValue.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                DateTime startDateValue = startDate ?? DateTime.MinValue;

                var res = await _db.Transactions
                    .Include(t => t.Payment)
                    .Include(t => t.Deposit)
                    .Include(t => t.UserTopUp)
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

        public async Task<Return<int>> GetRevenueTodayAsync()
        {
            try
            {
                var dateTimeNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var result = await _db.Transactions
                    .Where(t =>
                        t.CreatedDate.Date == dateTimeNow.Date &&
                        t.TransactionStatus.Equals(StatusTransactionEnum.SUCCEED) &&
                        t.PaymentId != null &&
                        t.DepositId == null
                    )
                    .SumAsync(t => t.Amount);
                return new Return<int>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<int>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<StatisticParkingAreaRevenueResDto>>> GetListStatisticParkingAreaRevenueAsync()
        {
            try
            {
                // Get total revenue of each parking area
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var revenueData = await _db.Transactions
                    .Where(t => t.Payment != null && t.Payment.Session != null && t.Payment.Session.GateIn != null && t.TransactionStatus == StatusTransactionEnum.SUCCEED)
                    .GroupBy(t => t.Payment.Session.GateIn.ParkingAreaId)
                    .Select(g => new
                    {
                        ParkingAreaId = g.Key,
                        Revenue = g.Sum(t => t.Amount)
                    })
                    .ToListAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                // Get all parking areas
                var parkingAreas = await _db.ParkingAreas.Where(t => t.DeletedDate == null).ToListAsync();

                // Join the revenue data with parking areas
                var result = parkingAreas
                    .GroupJoin(
                        revenueData,
                        p => p.Id,
                        r => r.ParkingAreaId,
                        (p, r) => new StatisticParkingAreaRevenueResDto
                        {
                            ParkingArea = new GetParkingAreaOptionResDto
                            {
                                Id = p.Id,
                                Description = p.Description ?? string.Empty,
                                Name = p.Name,
                            },
                            Revenue = r.FirstOrDefault()?.Revenue ?? 0
                        })
                    .ToList();
                return new Return<IEnumerable<StatisticParkingAreaRevenueResDto>>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<StatisticParkingAreaRevenueResDto>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<Transaction>> GetTransactionBySessionIdAsync(Guid sessionId)
        {
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var result = await _db.Transactions
                    .Include(t => t.Payment)
                    .Where(t => t.Payment.SessionId.Equals(sessionId))
                    .FirstOrDefaultAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                return new Return<Transaction>
                {
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = result,
                    IsSuccess = true
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

        public async Task<Return<IEnumerable<StatisticRevenueParkingAreasDetailsResDto>>> GetListStatisticRevenueParkingAreasDetailsAsync(GetListObjectWithFillerDateAndSearchInputResDto req)
        {
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var revenueData = await _db.Transactions
                    .Where(t => t.TransactionStatus == StatusTransactionEnum.SUCCEED && t.Payment.Session.GateOut != null)
                    .GroupBy(t => t.Payment.Session.GateOut.ParkingAreaId)
                    .Select(g => new
                    {
                        ParkingAreaId = g.Key,
                        RevenueApp = g.Where(t => t.WalletId != null).Sum(t => t.Amount),
                        RevenueOther = g.Where(t => t.WalletId == null).Sum(t => t.Amount),
                        RevenueTotal = g.Sum(t => t.Amount),
                    })
                    .ToListAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                // Get all parking areas
                var parkingAreas = await _db.ParkingAreas
                    .Where(p => p.DeletedDate == null)
                    .ToListAsync();

                var result = parkingAreas
                    .GroupJoin(
                        revenueData,
                        p => p.Id,
                        r => r.ParkingAreaId,
                        (p, r) => new StatisticRevenueParkingAreasDetailsResDto
                        {
                            ParkingAreaId = p.Id,
                            ParkingAreaName = p.Name,
                            RevenueTotal = r.FirstOrDefault()?.RevenueTotal ?? 0,
                            RevenueApp = r.FirstOrDefault()?.RevenueApp ?? 0,
                            RevenueOther = r.FirstOrDefault()?.RevenueOther ?? 0
                        })
                    .ToList();
                return new Return<IEnumerable<StatisticRevenueParkingAreasDetailsResDto>>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<StatisticRevenueParkingAreasDetailsResDto>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }
    }
}
