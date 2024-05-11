using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    public class SessionRepository : ISessionRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public SessionRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Session>> CreateSessionAsync(Session session)
        {
            try
            {
                await _db.Sessions.AddAsync(session);
                await _db.SaveChangesAsync();
                return new Return<Session>
                {
                    Data = session,
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            } catch (Exception e)
            {
                return new Return<Session>
                {
                    IsSuccess = false,
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
