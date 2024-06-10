using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

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
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            } catch (Exception e)
            {
                return new Return<Session>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<Session>> GetSessionByCardIdAsync(Guid cardId)
        {
            try
            {
                return new Return<Session>
                {
                    Data = await _db.Sessions.FirstOrDefaultAsync(x => x.CardId == cardId),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            } catch (Exception e)
            {
                return new Return<Session>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<Session>> GetSessionByPlateNumberAsync(string plateNumber)
        {
            try
            {
                return new Return<Session>
                {
                    Data = await _db.Sessions.FirstOrDefaultAsync(x => x.PlateNumber == plateNumber),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            } catch (Exception e)
            {
                return new Return<Session>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
