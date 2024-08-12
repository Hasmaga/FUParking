using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.Session;
using FUParkingModel.ResponseObject.Statistic;
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
                    Message = ErrorEnumApplication.SERVER_ERROR,
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
                    Message = ErrorEnumApplication.SERVER_ERROR,
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
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Session>> GetNewestSessionByCardIdAsync(Guid cardId)
        {
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var result = await _db.Sessions
                    .Include(x => x.Card)
                    .Include(x => x.GateIn)
                    .Include(x => x.GateOut)
                    .Include(x => x.VehicleType)
                    .Include(x => x.PaymentMethod)
                    .Include(x => x.Customer)
                    .Include(x => x.Customer.CustomerType)
                    .Include(x => x.CreateBy)
                    .Include(x => x.LastModifyBy)
                    .Where(x => x.CardId == cardId && x.DeletedDate == null)
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefaultAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
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
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Session>> GetNewestSessionByPlateNumberAsync(string plateNumber)
        {
            try
            {
                var result = await _db.Sessions
                    .Where(x => x.PlateNumber == plateNumber && x.DeletedDate == null)
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefaultAsync();
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
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Session>> UpdateSessionAsync(Session session)
        {
            try
            {
                _db.Sessions.Update(session);
                await _db.SaveChangesAsync();
                return new Return<Session>
                {
                    Data = session,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
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

        public async Task<Return<IEnumerable<Session>>> GetListSessionByCustomerIdAsync(Guid customerId, DateTime? startDate, DateTime? endDate, int pageSize, int pageIndex)
        {
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var query = _db.Sessions
                    .Where(x => x.CustomerId == customerId && (x.Status == SessionEnum.CLOSED || x.Status == SessionEnum.PARKED))
                    .Include(x => x.GateIn)
                    .Include(x => x.GateOut)
                    .Include(x => x.VehicleType)
                    .Include(x => x.PaymentMethod)
                    .Include(x => x.GateIn.ParkingArea)
                    .Include(x => x.Feedbacks)
                    .AsQueryable();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                // Date filtering
                if (startDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate <= endDate.Value);
                }

                // Pagination
                var paginatedResult = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .ToListAsync();

                return new Return<IEnumerable<Session>>
                {
                    Data = paginatedResult,
                    IsSuccess = true,
                    TotalRecord = query.Count(),
                    Message = paginatedResult.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Session>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<Session>>> GetListSessionActiveByParkingIdAsync(Guid parkingId)
        {
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var result = await _db.Sessions
                    .Where(p => p.GateIn.ParkingAreaId == parkingId && p.Status == SessionEnum.PARKED)
                    .ToListAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                return new Return<IEnumerable<Session>>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Session>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<StatisticSessionAppResDto>>> StatisticSessionAppAsync()
        {
            try
            {
                var datetimeend = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var datetimestart = datetimeend.AddDays(-30);
                // Get total session each day from datetimestart to datetimeend and group by date
                var result = await _db.Sessions
                    .Where(x => x.CreatedDate >= datetimestart && x.CreatedDate <= datetimeend && x.Status.Equals(SessionEnum.CLOSED))
                    .GroupBy(x => x.CreatedDate.Date)
                    .Select(x => new StatisticSessionAppResDto
                    {
                        Date = x.Key,
                        TotalSession = x.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                return new Return<IEnumerable<StatisticSessionAppResDto>>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<StatisticSessionAppResDto>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        // Supervisor/manager want to see the session list
        // To support suppervisor checkout function
        public async Task<Return<IEnumerable<Session>>> GetListSessionAsync(GetListObjectWithFillerAttributeAndDateReqDto req)
        {          
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var query = _db.Sessions
                    .Include(t => t.GateIn)
                    .Include(t => t.GateOut)
                    .Include(t => t.PaymentMethod)
                    .Include(t => t.CreateBy)
                    .Include(t => t.LastModifyBy)
                    .Include(t => t.VehicleType)
                    .Include(t => t.GateIn.ParkingArea)
                    .Include(t => t.Customer)
                    .Where(p => p.DeletedDate != null)
                    .AsQueryable();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                if (req.SearchInput is not null && req.SearchInput is not null)
                {
                    switch (req.SearchInput.ToLower()) {
                        case "platenumber":
                            query = query.Where(p => p.PlateNumber.Contains(req.SearchInput));
                            break;

                        case "cardnumber":
                            query = query.Where(p => p.PlateNumber.Contains(req.SearchInput)); 
                            break;

                        case "customeremail":
                            query = query.Where(p => p.PlateNumber.Contains(req.SearchInput));
                            break;

                        case "parkingarea":
                            query = query.Where(p => p.PlateNumber.Contains(req.SearchInput));
                            break;

                        default:                             
                            break;
                    }
                }

                req.StartDate ??= DateTime.MinValue;

                req.EndDate ??= TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));                

                var result = await query
                    .Where(p => p.CreatedDate >= req.StartDate && p.CreatedDate <= req.EndDate)
                    .OrderByDescending(t => t.CreatedDate)
                    .Skip((req.PageIndex - 1) * req.PageSize)
                    .Take(req.PageSize)
                    .ToListAsync();

                return new Return<IEnumerable<Session>>
                {
                    Data = result,
                    TotalRecord = query.Count(),
                    Message = query.Any() ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = true,
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Session>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        
    }
}
