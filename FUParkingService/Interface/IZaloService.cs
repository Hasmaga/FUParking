using FUParkingModel.RequestObject.Zalo;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IZaloService
    {
        Task<Return<ZaloResDto>> CustomerCreateRequestBuyPackageByZaloPayAsync(Guid packageId);
        Task<Return<bool>> CallbackZaloPayAsync(string app_trans_id);
    }
}
