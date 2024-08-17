using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
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
    }
}
