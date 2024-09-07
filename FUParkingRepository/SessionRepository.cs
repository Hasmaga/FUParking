using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var result = await _db.Sessions
                    .Include(t => t.GateIn)
                    .Include(t => t.GateOut)
                    .Include(t => t.PaymentMethod)
                    .Include(t => t.CreateBy)
                    .Include(t => t.LastModifyBy)
                    .Include(t => t.VehicleType)
                    .Include(t => t.GateIn.ParkingArea)
                    .Include(t => t.Customer)
                    .Include(t => t.Card)
                    .FirstOrDefaultAsync(e => e.Id.Equals(sessionId));
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var result = await _db.Sessions
                    .Where(x => x.PlateNumber == plateNumber && x.DeletedDate == null)
                    .Include(x => x.Card)
                    .Include(x => x.GateIn)
                    .Include(x => x.GateOut)
                    .Include(x => x.VehicleType)
                    .Include(x => x.PaymentMethod)
                    .Include(x => x.Customer)
                    .Include(x => x.Customer.CustomerType)
                    .Include(x => x.CreateBy)
                    .Include(x => x.LastModifyBy)
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
                    .Include(t => t.Card)
                    .Where(p => p.DeletedDate == null)
                    .AsQueryable();

                if (req.SearchInput is not null && req.Attribute is not null)
                {
                    switch (req.Attribute.ToLower()) {
                        case "platenumber":
                            query = query.Where(p => p.PlateNumber.Contains(req.SearchInput));
                            break;

                        case "cardnumber":
                            query = query.Where(p => p.Card.CardNumber.Contains(req.SearchInput)); 
                            break;

                        case "customeremail":
                            query = query.Where(p => p.Customer.Email.Contains(req.SearchInput));
                            break;                        

                        default:                             
                            break;
                    }
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
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

        public async Task<Return<int>> GetTotalSessionParkedAsync()
        {
            try
            {
                var datetimenow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var result = await _db.Sessions
                    .Where(x => x.Status.Equals(SessionEnum.PARKED))
                    .CountAsync();
                return new Return<int>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<int>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<double>> GetAverageSessionDurationPerDayAsync()
        {
            try
            {
                var dateTimeNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var sessions = await _db.Sessions
                    .Where(x => x.Status.Equals(SessionEnum.CLOSED) && x.CreatedDate.Date == dateTimeNow.Date && x.CreatedDate.Month == dateTimeNow.Month && x.CreatedDate.Year == dateTimeNow.Year)
                    .ToListAsync();

                if (sessions.Count == 0)
                {
                    return new Return<double>
                    {
                        Data = 0,
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                    };
                }

                var result = sessions
                    .GroupBy(x => x.CreatedDate.Date)
                    .Select(x => new
                    {
                        Date = x.Key,
                        AverageDuration = x.Average(y => (y.TimeOut.GetValueOrDefault() - y.TimeIn).TotalMinutes) / 60 
                    })
                    .Average(x => x.AverageDuration);
                return new Return<double> 
                {
                    Data = result,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<double>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<StatisticCheckInCheckOutResDto>> GetStatisticCheckInCheckOutAsync()
        {
            try
            {
                var datetimenow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var totalCheckIn = await _db.Sessions
                    .Where(x => x.CreatedDate.Date == datetimenow.Date && x.Status.Equals(SessionEnum.PARKED))
                    .CountAsync();
                var totalCheckOut = await _db.Sessions
                    .Where(x => x.CreatedDate.Date == datetimenow.Date && x.Status.Equals(SessionEnum.CLOSED))
                    .CountAsync();

                return new Return<StatisticCheckInCheckOutResDto> 
                { 
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, 
                    Data = new StatisticCheckInCheckOutResDto 
                    { 
                        TotalCheckInToday = totalCheckIn, 
                        TotalCheckOutToday = totalCheckOut 
                    }, 
                    IsSuccess = true 
                };
            }
            catch (Exception e)
            {
                return new Return<StatisticCheckInCheckOutResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<StatisticSessionTodayResDto>> GetStatisticCheckInCheckOutInParkingAreaAsync(Guid parkingId)
        {
            try
            {
                var datetimenow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var totalCheckIn = await _db.Sessions
                    .Include(p => p.GateIn)
                    .Where(x => x.CreatedDate.Date == datetimenow.Date
                                && x.Status != SessionEnum.CANCELLED
                                && x.GateIn != null
                                && x.GateIn.ParkingAreaId == parkingId)
                    .CountAsync();


                var totalCheckOut = await _db.Sessions
                    .Include(p => p.GateOut)
                    .Where(x => x.CreatedDate.Date == datetimenow.Date 
                        && x.GateOut != null
                        && x.GateOut.ParkingAreaId.Equals(parkingId))
                    .CountAsync();

                var totalVehicleParked = await _db.Sessions
                    .Include(p => p.GateIn)
                    .Where(x => x.Status.Equals(SessionEnum.PARKED)
                        && x.GateIn != null
                        && x.GateIn.ParkingAreaId.Equals(parkingId))
                    .CountAsync();

                var totalLot = await _db.ParkingAreas
                    .Where(x => x.Id.Equals(parkingId))
                    .Select(x => x.MaxCapacity)
                    .FirstOrDefaultAsync();

                return new Return<StatisticSessionTodayResDto>
                {
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data = new StatisticSessionTodayResDto
                    {
                        TotalCheckInToday = totalCheckIn,
                        TotalCheckOutToday = totalCheckOut,
                        TotalVehicleParked = totalVehicleParked,
                        TotalLot = totalLot
                    },
                    IsSuccess = true
                };
            }
            catch (Exception e)
            {
                return new Return<StatisticSessionTodayResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<Session>>> GetAllSessionByCardNumberAndPlateNumberAsync(Guid parkingId, string? plateNum, string? cardNum, string? statusFilter, int pageIndex, int pageSize, DateTime? startDate, DateTime? endDate)
        {
            Return<IEnumerable<Session>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                var datetimenow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                if (startDate.HasValue && endDate.HasValue)
                {
                    if (startDate > endDate)
                    {
                        (startDate, endDate) = (endDate, startDate);
                    }
                }
                else if (startDate.HasValue && !endDate.HasValue)
                {
                    endDate = datetimenow.Date.AddDays(1).AddTicks(-1);  // endDate = dd/MM/yyyy 23:59:59.9999999
                }
                else if (!startDate.HasValue && endDate.HasValue)
                {
                    startDate = endDate.Value.Date;
                }
                else
                {
                    startDate = datetimenow.Date;
                    endDate = datetimenow.Date.AddDays(1).AddTicks(-1);  // endDate = dd/MM/yyyy 23:59:59.9999999
                }
                
                var query = _db.Sessions
                    .Include(t => t.GateIn)
                    .Include(t => t.GateOut)
                    .Include(t => t.PaymentMethod)
                    .Include(t => t.VehicleType)
                    .Include(t => t.Customer)
                    .Include(t => t.Card)
                    .Where(p => p.DeletedDate == null
                        && p.TimeIn <= endDate.GetValueOrDefault()
                        && p.TimeIn >= startDate.GetValueOrDefault())
                    .AsQueryable();

                if (!string.IsNullOrEmpty(plateNum))
                {
                    query = query.Where(p => p.PlateNumber.Contains(plateNum));
                }
                if (!string.IsNullOrEmpty(cardNum))
                {
                    query = query.Where(p => p.Card!.CardNumber.Contains(cardNum));
                }
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    query = query.Where(p => p.Status.Contains(statusFilter));
                }

                // Get total record count before pagination
                int totalRecords = await query.CountAsync();

                var result = await query
                    .OrderByDescending(t => t.Status == SessionEnum.PARKED)
                    .ThenByDescending(t => t.CreatedDate)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new Return<IEnumerable<Session>>
                {
                    Data = result,
                    TotalRecord = totalRecords,
                    Message = result.Any() ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
                return res;
            }
        }

        public async Task<Return<Session>> GetNewestSessionByCardNumberAsync(string cardNumber)
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
                    .Where(x => x.Card.CardNumber == cardNumber && x.DeletedDate == null)
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
    }
}
