using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IVehicleService
    {
        Task<Return<IEnumerable<VehicleType>>> GetVehicleTypesAsync();
    }
}
