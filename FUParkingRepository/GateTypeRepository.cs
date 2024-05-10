using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class GateTypeRepository : IGateTypeRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public GateTypeRepository(FUParkingDatabaseContext db)
        {
            _db = db;
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
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<GateType>()
                {
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
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
                    SuccessfullyMessage = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GateType>>
                {
                    ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }
    }
}
