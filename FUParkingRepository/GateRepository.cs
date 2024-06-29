using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
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

        public async Task<Return<IEnumerable<Gate>>> GetAllGateAsync()
        {
            try
            {
                var result = await _db.Gates.Where(t => t.DeletedDate == null).ToListAsync();
                return new Return<IEnumerable<Gate>>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    TotalRecord = result.Count
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
                var result = await _db.Gates.FirstOrDefaultAsync(p => p.Id.Equals(id));               
                res.Data = result;
                res.Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT;
                res.IsSuccess = true;
                return res;
            }
            catch(Exception ex)
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
    }
}
