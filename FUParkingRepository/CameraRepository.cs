using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    public class CameraRepository : ICameraRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public CameraRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Camera>> CreateCameraAsync(Camera camera)
        {
            try
            {
                await _db.Cameras.AddAsync(camera);
                await _db.SaveChangesAsync();
                return new Return<Camera>
                {
                    Data = camera,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Camera>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GOOGLE_LOGIN_FAILED,
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
