using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    public class PriceTableRepository : IPriceTableRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public PriceTableRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<PriceTable>> CreatePriceTableAsync(PriceTable priceTable)
        {
            try
            {
                await _db.PriceTables.AddAsync(priceTable);
                await _db.SaveChangesAsync();
                return new Return<PriceTable>
                {
                    Data = priceTable,
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            } catch (Exception e)
            {
                return new Return<PriceTable>
                {
                    IsSuccess = false,
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
