﻿using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IVehicleRepository
    {
        Task<Return<Vehicle>> CreateVehicleAsync(Vehicle vehicle);
        Task<Return<VehicleType>> CreateVehicleTypeAsync(VehicleType vehicleType);
        Task<Return<VehicleType>> GetVehicleTypeByIdAsync(Guid vehicleTypeId);
        Task<Return<VehicleType>> UpdateVehicleTypeAsync(VehicleType vehicleType);
        Task<Return<IEnumerable<VehicleType>>> GetAllVehicleTypeAsync();
    }
}
