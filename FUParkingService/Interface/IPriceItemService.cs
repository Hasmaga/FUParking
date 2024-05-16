using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPriceItemService
    {
        Task<Return<bool>> CreatePriceItemAsync(CreatePriceItemResDto req);
        Task<Return<bool>> DeletePriceItemAsync(Guid id);
    }
}
