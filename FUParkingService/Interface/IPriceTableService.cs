using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPriceTableService
    {
        Task<Return<bool>> CreatePriceTableAsync(CreatePriceTableResDto req);
    }
}
