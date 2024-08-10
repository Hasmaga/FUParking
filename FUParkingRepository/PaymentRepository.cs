using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Return<IEnumerable<StatisticPaymentByCustomerResDto>>> StatisticPaymentByCustomerAsync(Guid customerId)
        {
            try
            {
                var datetimeend = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var datetimestart = datetimeend.AddDays(-30);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var result = await _db.Payments
                    .Include(t => t.Session.Customer)
                    .Where(t => t.Session.CustomerId == customerId && t.CreatedDate >= datetimestart && t.CreatedDate <= datetimeend)
                    .GroupBy(t => new { t.CreatedDate.Date })
                    .Select(t => new StatisticPaymentByCustomerResDto
                    {
                        Date = t.Key.Date,
                        Amount = t.Sum(x => x.TotalPrice),
                        TotalPayment = t.Count()
                    })
                    .ToListAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                return new Return<IEnumerable<StatisticPaymentByCustomerResDto>>
                {
                    Message = SuccessfullyEnumServer.SUCCESSFULLY,
                    Data = result,
                    IsSuccess = true,
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<StatisticPaymentByCustomerResDto>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }
        public async Task<Return<IEnumerable<StatisticSessionPaymentMethodByCustomerResDto>>> StatisticSessionPaymentMethodByCustomerAsync(Guid customerId)
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

                var resultWallet = new StatisticSessionPaymentMethodByCustomerResDto()
                {
                    PaymentMethod = PaymentMethods.WALLET,
                    TotalPayment = resultListPaymentWallet.Count,
                };

                var resultCash = new StatisticSessionPaymentMethodByCustomerResDto()
                {
                    PaymentMethod = PaymentMethods.CASH,
                    TotalPayment = resultListPaymentCash.Count,
                };

                return new Return<IEnumerable<StatisticSessionPaymentMethodByCustomerResDto>>
                {
                    Message = SuccessfullyEnumServer.SUCCESSFULLY,
                    Data = [resultWallet, resultCash],
                    IsSuccess = true,
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<StatisticSessionPaymentMethodByCustomerResDto>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }
    }
}
