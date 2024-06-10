using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IParkingAreaRepository
    {
        Task<Return<ParkingArea>> CreateParkingAreaAsync(ParkingArea parkingArea);
        Task<Return<IEnumerable<ParkingArea>>> GetParkingAreasAsync();
        Task<Return<ParkingArea>> UpdateParkingAreaAsync(ParkingArea parkingArea);
        Task<Return<ParkingArea>> GetParkingAreaByNameAsync(string name);        
        Task<Return<ParkingArea>> GetParkingAreaByIdAsync(Guid parkingId);
        Task<Return<IEnumerable<ParkingArea>>> GetAllParkingAreasAsync(int pageIndex, int pageSize);
        Task<Return<ParkingArea>> GetParkingAreaByGateIdAsync(Guid gateId);
    }
}
