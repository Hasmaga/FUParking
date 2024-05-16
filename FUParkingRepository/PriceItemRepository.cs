using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    public class PriceItemRepository : IPriceItemRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public PriceItemRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<PriceItem>> CreatePriceItemAsync(PriceItem priceItem)
        {
            try
            {
                await _db.PriceItems.AddAsync(priceItem);
                await _db.SaveChangesAsync();
                return new Return<PriceItem>
                {
                    Data = priceItem,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            } catch (Exception e)
            {
                return new Return<PriceItem>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<bool>> DeletePriceItemAsync(PriceItem priceItem)
        {
            try
            {
                _db.PriceItems.Remove(priceItem);
                await _db.SaveChangesAsync();
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
                };
            } catch (Exception e)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.DELETE_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<PriceItem>> GetPriceItemByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
