using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Session;
using FUParkingModel.ResponseObject.Session;
using FUParkingModel.ResponseObject.SessionCheckOut;
using FUParkingModel.ReturnCommon;
using Microsoft.AspNetCore.Http;

namespace FUParkingService.Interface
{
    public interface ISessionService
    {
        Task<Return<dynamic>> CheckInAsync(CreateSessionReqDto req);
        Task<Return<bool>> CheckInForGuestAsync(string PlateNumber, Guid CardId, Guid GateInId, IFormFile ImageIn, Guid VehicleType);
        Task<Return<dynamic>> UpdatePaymentSessionAsync(string CardNumber);
        Task<Return<CheckOutResDto>> CheckOutAsync(string CardNumber, Guid GateOutId, DateTime TimeOut, IFormFile ImageOut);
        Task<Return<IEnumerable<GetHistorySessionResDto>>> GetListSessionByCustomerAsync(GetListObjectWithFillerDateReqDto req);
    }
}
