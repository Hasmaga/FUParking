using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FUParkingRepository
{
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public PaymentMethodRepository(FUParkingDatabaseContext db)
        {
            _db = db;
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
                    IsSuccess = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<IEnumerable<PaymentMethod>>> GetAllPaymentMethodAsync()
        {
            try
            {
                return new Return<IEnumerable<PaymentMethod>>
                {
                    Data = await _db.PaymentMethods.ToListAsync(),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            } 
            catch (Exception e)
            {
                return new Return<IEnumerable<PaymentMethod>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<PaymentMethod>> GetPaymentMethodByIdAsync(Guid paymentMethodId)
        {
            try
            {
                return new Return<PaymentMethod>
                {
                    Data = await _db.PaymentMethods.FindAsync(paymentMethodId),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            } 
            catch (Exception e)
            {
                return new Return<PaymentMethod>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
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
                    IsSuccess = false,
                    Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
