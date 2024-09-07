using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ResponseObject.Payment;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FUParkingRepository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public PaymentRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Payment>> CreatePaymentAsync(Payment payment)
        {
            try
            {
                await _db.Payments.AddAsync(payment);
                await _db.SaveChangesAsync();
                return new Return<Payment>
                {
                    Data = payment,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Payment>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<PaymentMethod>> CreatePaymentMethodAsync(PaymentMethod paymentMethod)
        {
            try
            {
                await _db.PaymentMethods.AddAsync(paymentMethod);
                await _db.SaveChangesAsync();
                return new Return<PaymentMethod>
                {
                    Data = paymentMethod,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<PaymentMethod>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<PaymentMethod>>> GetAllPaymentMethodAsync()
        {
            try
            {
                var result = await _db.PaymentMethods.ToListAsync();
                return new Return<IEnumerable<PaymentMethod>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = result.Count,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<PaymentMethod>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<PaymentMethod>> GetPaymentMethodByIdAsync(Guid paymentMethodId)
        {
            try
            {
                var result = await _db.PaymentMethods.FindAsync(paymentMethodId);
                return new Return<PaymentMethod>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<PaymentMethod>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<PaymentMethod>> GetPaymentMethodByNameAsync(string name)
        {
            try
            {
                var result = await _db.PaymentMethods
                    .Where(t => t.Name != null && t.Name.ToLower().Equals(name.ToLower()))
                    .FirstOrDefaultAsync();
                return new Return<PaymentMethod>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<PaymentMethod>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<PaymentMethod>> UpdatePaymentMethodAsync(PaymentMethod paymentMethod)
        {
            try
            {
                _db.PaymentMethods.Update(paymentMethod);
                await _db.SaveChangesAsync();
                return new Return<PaymentMethod>
                {
                    Data = paymentMethod,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<PaymentMethod>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Payment>> GetPaymentBySessionIdAsync(Guid sessionId)
        {
            try
            {
                var result = await _db.Payments
                    .Where(t => t.SessionId == sessionId)
                    .FirstOrDefaultAsync();
                return new Return<Payment>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<Payment>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<StatisticPaymentByCustomerResDto>> StatisticPaymentByCustomerAsync(Guid customerId)
        {
            try
            {
                var datetimee = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                // Get TotalPaymentInThisMonth and TotalTimePakedInThisMonth 
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var result = await _db.Payments
                    .Include(t => t.Session.Customer)
                    .Where(t => t.Session.CustomerId == customerId && t.CreatedDate.Month == datetimee.Month && t.CreatedDate.Year == datetimee.Year)
                    .GroupBy(t => t.Session.CustomerId)
                    .Select(t => new StatisticPaymentByCustomerResDto
                    {                        
                        TotalPaymentInThisMonth = t.Sum(x => x.TotalPrice),
                        TotalTimePakedInThisMonth = t.Count()
                    })
                    .FirstOrDefaultAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                return new Return<StatisticPaymentByCustomerResDto>
                {
                    Message = SuccessfullyEnumServer.SUCCESSFULLY,
                    Data = result,
                    IsSuccess = true,
                };
            }
            catch (Exception e)
            {
                return new Return<StatisticPaymentByCustomerResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }
        public async Task<Return<IEnumerable<StatisticSessionPaymentMethodResDto>>> StatisticSessionPaymentMethodByCustomerAsync(Guid customerId)
        {
            try
            {
                // Get list payment by payment method of customer in current month
                var datetimenow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var resultListPaymentWallet = await _db.Payments
                    .Include(t => t.Session.Customer)
                    .Include(t => t.PaymentMethod)
                    .Where(t => t.Session.CustomerId == customerId && t.CreatedDate.Month == datetimenow.Month && t.CreatedDate.Year == datetimenow.Year && t.PaymentMethod.Name.Equals(PaymentMethods.WALLET))                    
                    .ToListAsync();

                var resultListPaymentCash = await _db.Payments
                    .Include(t => t.Session.Customer)
                    .Include(t => t.PaymentMethod)
                    .Where(t => t.Session.CustomerId == customerId && t.CreatedDate.Month == datetimenow.Month && t.CreatedDate.Year == datetimenow.Year && t.PaymentMethod.Name.Equals(PaymentMethods.CASH))                    
                    .ToListAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                var resultWallet = new StatisticSessionPaymentMethodResDto()
                {
                    PaymentMethod = PaymentMethods.WALLET,
                    TotalPayment = resultListPaymentWallet.Count,
                };

                var resultCash = new StatisticSessionPaymentMethodResDto()
                {
                    PaymentMethod = PaymentMethods.CASH,
                    TotalPayment = resultListPaymentCash.Count,
                };

                return new Return<IEnumerable<StatisticSessionPaymentMethodResDto>>
                {
                    Message = SuccessfullyEnumServer.SUCCESSFULLY,
                    Data = [resultWallet, resultCash],
                    IsSuccess = true,
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<StatisticSessionPaymentMethodResDto>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<StatisticSessionPaymentMethodResDto>>> StatisticSessionPaymentMethodAsync()
        {
            try
            {
                var datetimenow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var resultListPaymentWallet = await _db.Payments
                    .Include(t => t.Session.Customer)
                    .Include(t => t.PaymentMethod)
                    .Where(t => t.CreatedDate.Month == datetimenow.Month && t.CreatedDate.Year == datetimenow.Year && t.PaymentMethod.Name.Equals(PaymentMethods.WALLET))
                    .ToListAsync();

                var resultListPaymentCash = await _db.Payments
                    .Include(t => t.Session.Customer)
                    .Include(t => t.PaymentMethod)
                    .Where(t =>  t.CreatedDate.Month == datetimenow.Month && t.CreatedDate.Year == datetimenow.Year && t.PaymentMethod.Name.Equals(PaymentMethods.CASH))
                    .ToListAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                var resultWallet = new StatisticSessionPaymentMethodResDto()
                {
                    PaymentMethod = PaymentMethods.WALLET,
                    TotalPayment = resultListPaymentWallet.Count,
                };

                var resultCash = new StatisticSessionPaymentMethodResDto()
                {
                    PaymentMethod = PaymentMethods.CASH,
                    TotalPayment = resultListPaymentCash.Count,
                };

                return new Return<IEnumerable<StatisticSessionPaymentMethodResDto>>
                {
                    Message = SuccessfullyEnumServer.SUCCESSFULLY,
                    Data = [resultWallet, resultCash],
                    IsSuccess = true,
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<StatisticSessionPaymentMethodResDto>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<StatisticPaymentTodayResDto>> GetStatisticPaymentTodayForGateAsync(Guid gateId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var datetimenow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                startDate ??= datetimenow.Date;
                endDate ??= datetimenow.Date;

                if (startDate > endDate)
                {
                    if (startDate > endDate)
                    {
                        (startDate, endDate) = (endDate, startDate);
                    }
                }

                var totalWalletPayment = await _db.Payments
                    .Where(p => p.Session != null
                        && p.Session.GateOutId.Equals(gateId)
                        && p.PaymentMethod!.Name == PaymentMethods.WALLET
                        && p.CreatedDate.Date >= startDate.Value.Date
                        && p.CreatedDate.Date <= endDate.Value.Date)
                    .SumAsync(p => p.TotalPrice);

                var totalCashPayment = await _db.Payments
                    .Where(p => p.Session != null
                        && p.Session.GateOutId.Equals(gateId)
                        && p.PaymentMethod!.Name == PaymentMethods.CASH
                        && p.CreatedDate.Date >= startDate.Value.Date
                        && p.CreatedDate.Date <= endDate.Value.Date)
                    .SumAsync(p => p.TotalPrice);

                return new Return<StatisticPaymentTodayResDto>
                {
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data = new StatisticPaymentTodayResDto
                    {
                        TotalWalletPayment = totalWalletPayment,
                        TotalCashPayment = totalCashPayment,
                    },
                    IsSuccess = true
                };
            }
            catch (Exception e)
            {
                return new Return<StatisticPaymentTodayResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }
    }
}
