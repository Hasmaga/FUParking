using FUParkingModel.Object;
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
        Task<Return<dynamic>> CheckOutAsync(CheckOutAsyncReqDto req);
        Task<Return<IEnumerable<GetHistorySessionResDto>>> GetListSessionByCustomerAsync(GetListObjectWithFillerDateReqDto req);
        Task<Return<IEnumerable<StatisticSessionAppResDto>>> StatisticSessionAppAsync();
        Task<Return<dynamic>> CheckOutSessionByPlateNumberAsync(CheckOutSessionByPlateNumberReqDto req);
        Task<Return<IEnumerable<GetSessionByUserResDto>>> GetListSessionByUserAsync(GetListObjectWithFillerAttributeAndDateReqDto req);
        Task<Return<GetSessionByUserResDto>> GetSessionBySessionIdAsync(Guid sessionId);
        Task<Return<bool>> CancleSessionByIdAsync(Guid sessionId);
        Task<Return<int>> GetTotalSessionParkedAsync();
        Task<Return<double>> GetAverageSessionDurationPerDayAsync();
        Task<Return<StatisticCheckInCheckOutResDto>> GetStatisticCheckInCheckOutAsync();
        Task<Return<StatisticSessionTodayResDto>> GetStatisticCheckInCheckOutInParkingAreaAsync(Guid parkingId);
        Task<Return<IEnumerable<GetAllSessionTodayResDto>>> GetAllSessionByCardNumberAndPlateNumberAsync(Guid parkingId, string? plateNum, string? cardNum, int pageIndex, int pageSize, DateTime? startDate, DateTime? endDate);
        Task<Return<GetSessionByCardNumberResDto>> GetNewestSessionByCardNumberAsync(string CardNumber, DateTime TimeOut);
        Task<Return<GetSessionByPlateNumberResDto>> GetNewestSessionByPlateNumberAsync(string PlateNumber, DateTime TimeOut);
        Task<Return<dynamic>> UpdatePlateNumberInSessionAsync(UpdatePlateNumberInSessionReqDto req);
        Task<Return<GetCustomerTypeByPlateNumberResDto>> GetCustomerTypeByPlateNumberAsync(string PlateNumber);
    }
}
