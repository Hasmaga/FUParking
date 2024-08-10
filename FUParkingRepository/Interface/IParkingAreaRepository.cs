using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
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
        Task<Return<IEnumerable<ParkingArea>>> GetAllParkingAreasAsync(GetListObjectWithPageReqDto req);
        Task<Return<ParkingArea>> GetParkingAreaByGateIdAsync(Guid gateId);
    }
}
