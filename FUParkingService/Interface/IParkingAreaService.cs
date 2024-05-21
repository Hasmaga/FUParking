using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IParkingAreaService
    {
        Task<Return<bool>> CreateParkingAreaAsync(CreateParkingAreaReqDto req);
        //Task<Return<IEnumerable<ParkingArea>>> GetParkingAreasAsync();
        Task<Return<bool>> UpdateParkingAreaAsync(UpdateParkingAreaReqDto req);

    }
}
