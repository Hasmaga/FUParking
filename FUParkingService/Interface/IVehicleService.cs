using FUParkingModel.Object;
﻿using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.CustomerVehicle;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IVehicleService
    {
        Task<Return<IEnumerable<VehicleType>>> GetVehicleTypesAsync();
        Task<Return<bool>> CreateVehicleTypeAsync(CreateVehicleTypeReqDto reqDto);
        Task<Return<bool>> UpdateVehicleTypeAsync(UpdateVehicleTypeReqDto reqDto);
        Task<Return<IEnumerable<Vehicle>>> GetVehiclesAsync();
        Task<Return<List<Vehicle>>> GetCustomerVehicleByCustomerIdAsync(Guid customerGuid);
        Task<Return<bool>> DeleteVehicleTypeAsync(Guid id);
        Task<Return<bool>> CreateCustomerVehicleAsync(CreateCustomerVehicleReqDto reqDto);
        Task<Return<IEnumerable<VehicleType>>> GetAllVehicleTypePagingAsync(int pageSize, int pageIndex);
    }
}
