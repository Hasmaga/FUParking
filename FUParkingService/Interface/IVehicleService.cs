﻿using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IVehicleService
    {
        Task<Return<bool>> CreateVehicleType(CreateVehicleTypeReqDto reqDto);
    }
}
