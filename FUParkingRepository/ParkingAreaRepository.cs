using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

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
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            } catch (Exception e)
            {
                return new Return<ParkingArea>
                {
                    IsSuccess = false,
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
