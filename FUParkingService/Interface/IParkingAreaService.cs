using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.ParkingArea;
using FUParkingModel.ResponseObject.ParkingArea;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IParkingAreaService
    {
        Task<Return<dynamic>> CreateParkingAreaAsync(CreateParkingAreaReqDto req);
        Task<Return<dynamic>> DeleteParkingArea(Guid id);
        Task<Return<IEnumerable<GetParkingAreaReqDto>>> GetParkingAreasAsync(GetListObjectWithFiller req);
        Task<Return<dynamic>> UpdateParkingAreaAsync(UpdateParkingAreaReqDto req);
        Task<Return<dynamic>> UpdateStatusParkingAreaAsync(Guid parkingId, bool isActive);
        Task<Return<IEnumerable<GetParkingAreaOptionResDto>>> GetParkingAreaOptionAsync();
        Task<Return<dynamic>> CreateParkingAreaAndGateAsync(CreateParkingAreaAndGateReqDto req);
    }
}
