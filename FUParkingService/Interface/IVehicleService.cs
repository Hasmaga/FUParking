using FUParkingModel.Object;
﻿using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Vehicle;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IVehicleService
    {
        Task<Return<IEnumerable<VehicleType>>> GetVehicleTypesAsync(GetListObjectWithFiller req);
        Task<Return<bool>> CreateVehicleTypeAsync(CreateVehicleTypeReqDto reqDto);
        Task<Return<bool>> UpdateVehicleTypeAsync(Guid Id, UpdateVehicleTypeReqDto reqDto);
        Task<Return<IEnumerable<Vehicle>>> GetVehiclesAsync();
        Task<Return<List<Vehicle>>> GetCustomerVehicleByCustomerIdAsync(Guid customerGuid);
        Task<Return<bool>> DeleteVehicleTypeAsync(Guid id);
        Task<Return<bool>> CreateCustomerVehicleAsync(CreateCustomerVehicleReqDto reqDto);
    }
}
