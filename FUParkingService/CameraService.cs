using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class CameraService : ICameraService
    {
        private readonly ICameraRepository _cameraRepository;

        public CameraService(ICameraRepository cameraRepository)
        {
            _cameraRepository = cameraRepository;
        }
    }
}
