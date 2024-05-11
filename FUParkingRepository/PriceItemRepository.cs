using FUParkingModel.DatabaseContext;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    internal class PriceItemRepository : IPriceItemRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public PriceItemRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public Task<Return<PriceItem>> CreatePriceItemAsync(PriceItem priceItem)
        {
            throw new NotImplementedException();
        }
    }
}
