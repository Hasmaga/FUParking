using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IVehicleService
    {
        Task<Return<bool>> CreateVehicleTypeAsync(CreateVehicleTypeReqDto reqDto);
        Task<Return<bool>> UpdateVehicleTypeAsync(UpdateVehicleTypeReqDto reqDto);
    }
}
