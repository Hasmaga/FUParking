using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Gate;
using FUParkingModel.ResponseObject.Gate;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IGateService
    {
        Task<Return<IEnumerable<GetGateResDto>>> GetAllGateAsync(GetListObjectWithFiller req);
        Task<Return<dynamic>> UpdateGateAsync(UpdateGateReqDto req, Guid id);
        Task<Return<dynamic>> CreateGateAsync(CreateGateReqDto req);
        Task<Return<dynamic>> DeleteGate(Guid id);
        Task<Return<dynamic>> UpdateStatusGateAsync(Guid gateId, bool isActive);        
        Task<Return<IEnumerable<GetGateByParkingAreaResDto>>> GetListGateByParkingAreaAsync(Guid parkingAreaId);
        Task<Return<dynamic>> CreateGatesForParkingAreaByStaffAsync(CreateGatesForParkingAreaByStaffReqDto req);
        Task<Return<IEnumerable<GetGateByParkingAreaResDto>>> GetAllGateByParkingAreaAsync(Guid parkingAreaId);
        Task<Return<GetGateResDto>> GetGateByGateIdAsync(Guid id);
    }
}
