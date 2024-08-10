using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Session;
using FUParkingModel.ResponseObject.Session;
using FUParkingModel.ResponseObject.SessionCheckOut;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface ISessionService
    {
        Task<Return<dynamic>> CheckInAsync(CreateSessionReqDto req);
        Task<Return<bool>> CheckInForGuestAsync(CheckInForGuestReqDto req);
        Task<Return<dynamic>> UpdatePaymentSessionAsync(string CardNumber);
        Task<Return<CheckOutResDto>> CheckOutAsync(CheckOutAsyncReqDto req);
        Task<Return<IEnumerable<GetHistorySessionResDto>>> GetListSessionByCustomerAsync(GetListObjectWithFillerDateReqDto req);
        Task<Return<IEnumerable<StatisticSessionAppResDto>>> StatisticSessionAppAsync();        
    }
}
