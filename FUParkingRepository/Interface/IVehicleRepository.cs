using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IVehicleRepository
    {
        Task<Return<Vehicle>> CreateVehicleAsync(Vehicle vehicle);
        Task<Return<VehicleType>> CreateVehicleTypeAsync(VehicleType vehicleType);
        Task<Return<VehicleType>> GetVehicleTypeByIdAsync(Guid vehicleTypeId);
        Task<Return<VehicleType>> UpdateVehicleTypeAsync(VehicleType vehicleType);
        Task<Return<IEnumerable<VehicleType>>> GetAllVehicleTypeAsync(GetListObjectWithFiller? req = null);
        Task<Return<IEnumerable<Vehicle>>> GetVehiclesAsync(GetListObjectWithFillerAttributeAndDateReqDto req);
        Task<Return<IEnumerable<Vehicle>>> GetAllCustomerVehicleByCustomerIdAsync(Guid customerGuid);
        Task<Return<IEnumerable<Vehicle>>> GetVehiclesByVehicleTypeId(Guid id);
        Task<Return<VehicleType>> GetVehicleTypeByName(string VehicleTypeName);
        Task<Return<Vehicle>> GetVehicleByPlateNumberAsync(string PlateNumber);
        Task<Return<Vehicle>> GetVehicleByIdAsync(Guid vehicleId);
        Task<Return<Vehicle>> UpdateVehicleAsync(Vehicle vehicle);
        Task<Return<IEnumerable<VehicleType>>> GetAllVehicleTypeByCustomer();
        Task<Return<Vehicle>> GetNewestVehicleByVehicleTypeId(Guid vehicleTypeId);
        Task<Return<StatisticVehicleResDto>> GetStatisticVehicleAsync();
    }
}
