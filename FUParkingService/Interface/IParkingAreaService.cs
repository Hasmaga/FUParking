using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IParkingAreaService
    {
        Task<Return<bool>> CreateParkingAreaAsync(CreateParkingAreaReqDto req);
        Task<Return<bool>> ChangeStatusParkingAreaAsync(ChangeStatusParkingAreaReqDto req);
    }
}
