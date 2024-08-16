using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface ISessionRepository
    {
        Task<Return<Session>> CreateSessionAsync(Session session);
        Task<Return<Session>> GetSessionByCardIdAsync(Guid cardId);
        Task<Return<Session>> GetSessionByPlateNumberAsync(string plateNumber);
        Task<Return<Session>> GetSessionByIdAsync(Guid sessionId);
        Task<Return<Session>> GetNewestSessionByCardIdAsync(Guid cardId);
        Task<Return<Session>> GetNewestSessionByPlateNumberAsync(string plateNumber);
        Task<Return<Session>> UpdateSessionAsync(Session session);
        Task<Return<IEnumerable<Session>>> GetListSessionByCustomerIdAsync(Guid customerId, DateTime? startDate, DateTime? endDate, int pageSize, int pageIndex);
        Task<Return<IEnumerable<Session>>> GetListSessionActiveByParkingIdAsync(Guid parkingId);
        Task<Return<IEnumerable<StatisticSessionAppResDto>>> StatisticSessionAppAsync();
        Task<Return<IEnumerable<Session>>> GetListSessionAsync(GetListObjectWithFillerAttributeAndDateReqDto req);
        Task<Return<int>> GetTotalSessionParkingTodayAsync();
    }
}
