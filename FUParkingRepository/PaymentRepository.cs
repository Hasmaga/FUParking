using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
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
    }
}
