using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class VehicleTypeRepository : IVehicleTypeRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public VehicleTypeRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<VehicleType>> CreateVehicleTypeAsync(VehicleType vehicleType)
        {
            try
            {
                await _db.VehicleTypes.AddAsync(vehicleType);
                await _db.SaveChangesAsync();
                return new Return<VehicleType>()
                {
                    Data = vehicleType,
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<VehicleType>()
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<VehicleType>>> GetAllVehicleTypeAsync()
        {
            try
            {
                return new Return<IEnumerable<VehicleType>>()
                {
                    Data = await _db.VehicleTypes.ToListAsync(),
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<VehicleType>>()
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR
                };
            }
        }

        public async Task<Return<VehicleType>> GetVehicleTypeByIdAsync(Guid vehicleTypeId)
        {
            try
            {
                return new Return<VehicleType>()
                {
                    Data = await _db.VehicleTypes.FindAsync(vehicleTypeId),
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<VehicleType>()
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR
                };
            }
        }

        public async Task<Return<VehicleType>> UpdateVehicleTypeAsync(VehicleType vehicleType)
        {
            try
            {
                _db.VehicleTypes.Update(vehicleType);
                await _db.SaveChangesAsync();
                return new Return<VehicleType>()
                {
                    Data = vehicleType,
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<VehicleType>()
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    ErrorMessage = ErrorEnumApplication.UPDATE_OBJECT_ERROR
                };
            }
        }
    }
}
