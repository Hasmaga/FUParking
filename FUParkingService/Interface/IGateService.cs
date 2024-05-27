using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using Microsoft.Identity.Client;
using System.Threading.Tasks.Sources;

namespace FUParkingService.Interface
{
    public interface IGateService
    {
        Task<Return<IEnumerable<Gate>>> GetAllGate();
        Task<Return<bool>> UpdateGateAsync(UpdateGateReqDto req, Guid id);
    }
}
