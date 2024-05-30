﻿using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IGateService
    {
        Task<Return<IEnumerable<Gate>>> GetAllGate();
        Task<Return<bool>> UpdateGateAsync(UpdateGateReqDto req, Guid id);
        Task<Return<bool>> CreateGateAsync(CreateGateReqDto req);
        Task<Return<bool>> DeleteGate(Guid id);
    }
}
