using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IPriceItemRepository
    {
        Task<Return<PriceItem>> CreatePriceItemAsync(PriceItem priceItem);
    }
}
