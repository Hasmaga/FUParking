using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class GateRepository : IGateRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public GateRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Gate>> CreateGateAsync(Gate gate)
        {
            try
            {
                await _db.Gates.AddAsync(gate);
                await _db.SaveChangesAsync();
                return new Return<Gate>
                {
                    Data = gate,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Gate>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<Gate>>> GetAllGateAsync(GetListObjectWithFiller req)
        {
            try
            {
                var query = _db.Gates
                    .Include(p => p.GateType)
                    .Include(p => p.ParkingArea)
                    .Include(p => p.CreateBy)
                    .Include(p => p.LastModifyBy)
                    .Where(t => t.DeletedDate == null)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(req.Attribute) && !string.IsNullOrEmpty(req.SearchInput))
                {
                    switch (req.Attribute.ToUpper())
                    {
                        case "PARKINGAREANAME":
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                            query = query.Where(p => p.ParkingArea.Name.Contains(req.SearchInput));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                            break;
                        case "NAME":
                            query = query.Where(p => p.Name.Contains(req.SearchInput));
                            break;
                        case "STATUSGATE":
                            query = query.Where(p => p.StatusGate.Equals(req.SearchInput.ToUpper()));
                            break;
                    }
                }
                var result = await query
                    .OrderByDescending(t => t.CreatedDate)
                    .Skip((req.PageIndex - 1) * req.PageSize)
                    .Take(req.PageSize)
                    .ToListAsync();
                return new Return<IEnumerable<Gate>>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    TotalRecord = query.Count()
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Gate>>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<GateType>> CreateGateTypeAsync(GateType gateType)
        {
            try
            {
                await _db.GateTypes.AddAsync(gateType);
                await _db.SaveChangesAsync();
                return new Return<GateType>()
                {
                    Data = gateType,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<GateType>()
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<IEnumerable<GateType>>> GetAllGateTypeAsync()
        {
            try
            {
                var result = await _db.GateTypes.ToListAsync();
                return new Return<IEnumerable<GateType>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = result.Count,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GateType>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<Gate>> UpdateGateAsync(Gate gate)
        {
            try
            {
                _db.Gates.Update(gate);
                await _db.SaveChangesAsync();
                return new Return<Gate>
                {
                    Data = gate,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Gate>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Gate>> GetGateByIdAsync(Guid id)
        {
            Return<Gate> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                var result = await _db.Gates
                    .Include(p => p.GateType)
                    .Include(p => p.ParkingArea)
                    .Where(p => p.DeletedDate == null && p.Id.Equals(id))
                    .FirstOrDefaultAsync();
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

        public async Task<Return<Gate>> GetGateByNameAsync(string name)
        {
            try
            {
                var result = await _db.Gates.FirstOrDefaultAsync(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                return new Return<Gate>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<Gate> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<Gate>> GetGateTypeByIdAsync(Guid id)
        {
            try
            {
                var result = await _db.Gates.Where(t => t.DeletedDate == null).FirstOrDefaultAsync(p => p.Id.Equals(id));
                return new Return<Gate>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<Gate> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<Gate>> GetVirtualGateAsync()
        {
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var result = await _db.Gates.Include(p => p.GateType).FirstOrDefaultAsync(p => p.GateType.Name.Equals(GateTypeEnum.VIRUTAL));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                return new Return<Gate>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<Gate> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }
    }
}
