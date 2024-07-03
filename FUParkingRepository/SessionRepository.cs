using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
            }
            catch (Exception e)
            {
                return new Return<Session>
                {                    
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Session>> GetSessionByCardIdAsync(Guid cardId)
        {
            try
            {
                var result = await _db.Sessions.FirstOrDefaultAsync(x => x.CardId == cardId);
                return new Return<Session>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<Session>
                {                    
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Session>> GetSessionByPlateNumberAsync(string plateNumber)
        {
            try
            {
                var result = await _db.Sessions.Where(x => x.PlateNumber == plateNumber).FirstOrDefaultAsync();
                return new Return<Session>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<Session>
                {                    
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Session>> GetSessionByIdAsync(Guid sessionId)
        {
            try
            {
                var result = await _db.Sessions.Include(e => e.GateIn).FirstOrDefaultAsync(e => e.Id.Equals(sessionId));
                return new Return<Session>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<Session>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Session>> GetNewestSessionByCardIdAsync(Guid cardId)
        {
            try
            {
                var result = await _db.Sessions.Where(x => x.CardId == cardId).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                return new Return<Session>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<Session>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Session>> GetNewestSessionByPlateNumberAsync(string plateNumber)
        {
            try
            {
                var result = await _db.Sessions.Where(x => x.PlateNumber == plateNumber).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                return new Return<Session>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }           
            catch (Exception e)
            {
                return new Return<Session>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e
                };
            }
        }
    }
}
