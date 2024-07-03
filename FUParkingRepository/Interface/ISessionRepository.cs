using FUParkingModel.Object;
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
    }
}
