using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IParkingAreaService
    {
        Task<Return<bool>> CreateParkingAreaAsync(CreateParkingAreaReqDto req);
        Task<Return<bool>> DeleteParkingArea(Guid id);
        Task<Return<IEnumerable<ParkingArea>>> GetParkingAreasAsync(int pageIndex, int pageSize);
        Task<Return<bool>> UpdateParkingAreaAsync(UpdateParkingAreaReqDto req);
    }
}
