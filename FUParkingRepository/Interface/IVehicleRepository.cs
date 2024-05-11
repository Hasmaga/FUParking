using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IVehicleRepository
    {
        Task<Return<Vehicle>> CreateVehicleAsync(Vehicle vehicle);
    }
}
