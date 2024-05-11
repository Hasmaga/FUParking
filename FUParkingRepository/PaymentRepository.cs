using FUParkingModel.DatabaseContext;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public PaymentRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public Task<Return<Payment>> CreatePaymentAsync(Payment payment)
        {
            throw new NotImplementedException();
        }
    }
}
