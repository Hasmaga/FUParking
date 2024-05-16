using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPriceTableService
    {
        Task<Return<bool>> CreatePriceTableAsync(CreatePriceTableResDto req);
        Task<Return<bool>> UpdateStatusPriceTableAsync(ChangeStatusPriceTableResDto req);
        Task<Return<IEnumerable<GetPriceTableResDto>>> GetAllPriceTableAsync();
    }
}
