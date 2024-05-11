using FUParkingModel.DatabaseContext;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public PaymentMethodRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public Task<Return<PaymentMethod>> CreatePaymentMethodAsync(PaymentMethod paymentMethod)
        {
            throw new NotImplementedException();
        }
    }
}
