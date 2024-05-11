using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface ICameraRepository
    {
        Task<Return<Camera>> CreateCameraAsync(Camera camera);
    }
}
