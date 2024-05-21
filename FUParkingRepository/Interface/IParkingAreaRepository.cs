using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IParkingAreaRepository
    {
        Task<Return<ParkingArea>> CreateParkingAreaAsync(ParkingArea parkingArea);
        Task<Return<ParkingArea>> GetParkingAreaByNameAsync(string name);
        Task<Return<IEnumerable<ParkingArea>>> GetParkingAreasAsync();
    }
}
