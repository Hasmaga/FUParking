using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IGateService
    {
        Task<Return<IEnumerable<Gate>>> GetAllGateAsync();
        Task<Return<dynamic>> UpdateGateAsync(UpdateGateReqDto req, Guid id);
        Task<Return<dynamic>> CreateGateAsync(CreateGateReqDto req);
        Task<Return<dynamic>> DeleteGate(Guid id);
    }
}
