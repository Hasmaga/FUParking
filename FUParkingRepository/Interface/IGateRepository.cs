﻿using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IGateRepository
    {
        Task<Return<Gate>> CreateGateAsync(Gate gate);
        Task<Return<IEnumerable<Gate>>> GetAllGateAsync(GetListObjectWithFiller req);        
        Task<Return<Gate>> UpdateGateAsync(Gate gate);
        Task<Return<Gate>> GetGateByIdAsync(Guid id);
        Task<Return<Gate>> GetGateByNameAsync(string name);               
        Task<Return<IEnumerable<Gate>>> GetListGateByParkingAreaAsync(Guid parkingAreaId);
        Task<Return<IEnumerable<Gate>>> GetAllGateByParkingAreaAsync(Guid parkingAreaId);
        Task<Return<Gate>> GetGateByGateIdAsync(Guid id);
    }
}
