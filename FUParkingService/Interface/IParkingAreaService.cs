using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.ParkingArea;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IParkingAreaService
    {
        Task<Return<dynamic>> CreateParkingAreaAsync(CreateParkingAreaReqDto req);
        Task<Return<dynamic>> DeleteParkingArea(Guid id);
        Task<Return<IEnumerable<GetParkingAreaReqDto>>> GetParkingAreasAsync(int pageSize, int pageIndex, string? name);
        Task<Return<dynamic>> UpdateParkingAreaAsync(UpdateParkingAreaReqDto req);
    }
}
