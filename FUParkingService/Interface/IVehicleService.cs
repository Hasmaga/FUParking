﻿using FUParkingModel.Object;
﻿using FUParkingModel.RequestObject;
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
    }
}
