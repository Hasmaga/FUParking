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
                    IsSuccess = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<IEnumerable<Gate>>> GetAllGateAsync()
        {
            try
            {
                return new Return<IEnumerable<Gate>>
                {
                    Data = await _db.Gates.ToListAsync(),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Gate>>
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR
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
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<IEnumerable<GateType>>> GetAllGateTypeAsync()
        {
            try
            {
                return new Return<IEnumerable<GateType>>
                {
                    Data = await _db.GateTypes.ToListAsync(),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GateType>>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }
    }
}
