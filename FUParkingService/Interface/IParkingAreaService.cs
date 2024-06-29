using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IParkingAreaService
    {
        Task<Return<dynamic>> CreateParkingAreaAsync(CreateParkingAreaReqDto req);
        Task<Return<dynamic>> DeleteParkingArea(Guid id);
        Task<Return<IEnumerable<ParkingArea>>> GetParkingAreasAsync(int pageIndex, int pageSize);
        Task<Return<dynamic>> UpdateParkingAreaAsync(UpdateParkingAreaReqDto req);
    }
}
