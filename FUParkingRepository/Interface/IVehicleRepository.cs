using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IVehicleRepository
    {
        Task<Return<Vehicle>> CreateVehicleAsync(Vehicle vehicle);
        Task<Return<VehicleType>> CreateVehicleTypeAsync(VehicleType vehicleType);
        Task<Return<VehicleType>> GetVehicleTypeByIdAsync(Guid vehicleTypeId);
        Task<Return<VehicleType>> UpdateVehicleTypeAsync(VehicleType vehicleType);
        Task<Return<IEnumerable<VehicleType>>> GetAllVehicleTypeAsync(GetListObjectWithFiller req);
        Task<Return<IEnumerable<Vehicle>>> GetVehiclesAsync();
        Task<Return<List<Vehicle>>> GetAllCustomerVehicleByCustomerIdAsync(Guid customerGuid);
        Task<Return<IEnumerable<Vehicle>>> GetVehiclesByVehicleTypeId(Guid id);
        Task<Return<VehicleType>> GetVehicleTypeByName(string VehicleTypeName);
        Task<Return<IEnumerable<Vehicle>>> GetAllVehiclesAsync(int pageIndex, int pageSize);
    }
}
