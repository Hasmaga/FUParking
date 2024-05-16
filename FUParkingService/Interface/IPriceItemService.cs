using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPriceItemService
    {
        Task<Return<bool>> CreatePriceItemAsync(CreatePriceItemReqDto req);
        Task<Return<bool>> DeletePriceItemAsync(Guid id);
        Task<Return<IEnumerable<PriceItem>>> GetAllPriceItemByPriceTableAsync(Guid PriceTableId);
    }
}
