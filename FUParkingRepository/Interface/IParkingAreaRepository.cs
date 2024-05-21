using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IParkingAreaRepository
    {
        Task<Return<ParkingArea>> CreateParkingAreaAsync(ParkingArea parkingArea);
        Task<Return<IEnumerable<ParkingArea>>> GetParkingAreasAsync();
        Task<Return<ParkingArea>> UpdateParkingAreaAsync(ParkingArea parkingArea);
    }
}
