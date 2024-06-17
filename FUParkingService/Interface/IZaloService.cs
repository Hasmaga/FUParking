using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IZaloService
    {
        Task<Return<bool>> CustomerCreateRequestBuyPackageByZaloPayAsync(Guid packageId);
    }
}
