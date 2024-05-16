using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IPriceItemRepository
    {
        Task<Return<PriceItem>> CreatePriceItemAsync(PriceItem priceItem);
        Task<Return<bool>> DeletePriceItemAsync(PriceItem priceItem);
        Task<Return<PriceItem>> GetPriceItemByIdAsync(Guid id);
        Task<Return<IEnumerable<PriceItem>>> GetAllPriceItemByPriceTableAsync(Guid PriceTableId);
    }
}
