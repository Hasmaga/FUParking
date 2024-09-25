using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class ParkingAreaRepository : IParkingAreaRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public ParkingAreaRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<ParkingArea>> CreateParkingAreaAsync(ParkingArea parkingArea)
        {
            try
            {
                await _db.ParkingAreas.AddAsync(parkingArea);
                await _db.SaveChangesAsync();
                return new Return<ParkingArea>
                {
                    Data = parkingArea,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<ParkingArea>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<ParkingArea>> GetParkingAreaByIdAsync(Guid parkingId)
        {
            Return<ParkingArea> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                var result = await _db.ParkingAreas.Where(t => t.DeletedDate == null).FirstOrDefaultAsync(p => p.Id.Equals(parkingId));
                res.Data = result;
                res.Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT;
                res.IsSuccess = true;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
                return res;
            }
        }

        public async Task<Return<ParkingArea>> GetParkingAreaByNameAsync(string name)
        {
            try
            {
                var result = await _db.ParkingAreas.Where(r => r.DeletedDate == null && r.Name.ToLower().Equals(name.ToLower())).FirstOrDefaultAsync();
                return new Return<ParkingArea>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<ParkingArea>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<ParkingArea>>> GetParkingAreasAsync()
        {
            try
            {
                var result = await _db.ParkingAreas.Where(t => t.DeletedDate == null).ToListAsync();
                return new Return<IEnumerable<ParkingArea>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = result.Count,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<ParkingArea>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<ParkingArea>> UpdateParkingAreaAsync(ParkingArea parkingArea)
        {
            try
            {
                _db.ParkingAreas.Update(parkingArea);
                await _db.SaveChangesAsync();
                return new Return<ParkingArea>
                {
                    Data = parkingArea,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<ParkingArea>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<ParkingArea>>> GetAllParkingAreasAsync(GetListObjectWithFiller req)
        {
            try
            {
                var query = _db.ParkingAreas.Where(t => t.DeletedDate == null);
                if (req.Attribute is not null && req.SearchInput is not null)
                {
                    switch (req.Attribute)
                    {
                        case "name":
                            query = query.Where(t => t.Name.Contains(req.SearchInput));
                            break;
                        default:
                            break;
                    }
                }
                var result = await query
                    .OrderByDescending(t => t.CreatedDate)
                    .Skip((req.PageIndex - 1) * req.PageSize)
                    .Take(req.PageSize)
                    .ToListAsync();

                return new Return<IEnumerable<ParkingArea>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = query.Count(),
                    Message = query.Any() ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<ParkingArea>>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<ParkingArea>> GetParkingAreaByGateIdAsync(Guid gateId)
        {
            try
            {
                var query = from parkingArea in _db.ParkingAreas
                            join gate in _db.Gates on parkingArea.Id equals gate.ParkingAreaId
                            where gate.Id == gateId && parkingArea.DeletedDate == null
                            select parkingArea;
                var result = await query.FirstOrDefaultAsync();
                return new Return<ParkingArea> { Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT, IsSuccess = true, Data = result };
            }
            catch (Exception e)
            {
                return new Return<ParkingArea> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = e };
            }
        }

        public async Task<Return<IEnumerable<ParkingArea>>> GetParkingAreaOptionAsync()
        {
            try
            {
                var result = await _db.ParkingAreas.Where(t => t.DeletedDate == null).ToListAsync();
                return new Return<IEnumerable<ParkingArea>>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<ParkingArea>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<ParkingArea>> GetParkingAreaByParkingIdAsync(Guid id)
        {
            try
            {
                var result = await _db.ParkingAreas
                    .Where(p => p.Id.Equals(id) && p.DeletedDate == null)
                    .OrderByDescending(p => p.StatusParkingArea == StatusGateEnum.ACTIVE)
                    .ThenByDescending(p => p.CreatedDate)
                    .FirstOrDefaultAsync();

                return new Return<ParkingArea>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<ParkingArea>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }
    }
}
