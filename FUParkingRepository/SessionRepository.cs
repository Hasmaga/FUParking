using FUParkingModel.DatabaseContext;
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

        public Task<Return<Session>> CreateSessionAsync(Session session)
        {
            throw new NotImplementedException();
        }
    }
}
