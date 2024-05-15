using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPriceItemService
    {
        Task<Return<bool>> CreatePriceItem(CreatePriceItemResDto req);
    }
}
