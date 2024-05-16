using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPriceTableService
    {
        Task<Return<bool>> CreatePriceTableAsync(CreatePriceTableReqDto req);
        Task<Return<bool>> UpdateStatusPriceTableAsync(ChangeStatusPriceTableReqDto req);
        Task<Return<IEnumerable<GetPriceTableResDto>>> GetAllPriceTableAsync();
    }
}
