using FUParkingModel.RequestObject.Session;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface ISessionService
    {
        Task<Return<bool>> CheckInAsync(CreateSessionReqDto req);
    }
}
