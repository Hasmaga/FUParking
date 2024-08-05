using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Vehicle;
using FUParkingModel.ResponseObject.Vehicle;
using FUParkingModel.ResponseObject.VehicleType;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IVehicleService
    {
        Task<Return<IEnumerable<GetVehicleTypeByUserResDto>>> GetVehicleTypesAsync(GetListObjectWithFiller req);
        Task<Return<bool>> CreateVehicleTypeAsync(CreateVehicleTypeReqDto reqDto);
        Task<Return<bool>> UpdateVehicleTypeAsync(Guid Id, UpdateVehicleTypeReqDto reqDto);
        Task<Return<IEnumerable<GetVehicleForUserResDto>>> GetVehiclesAsync();
        Task<Return<IEnumerable<GetCustomerVehicleByCustomerResDto>>> GetCustomerVehicleByCustomerIdAsync();
        Task<Return<dynamic>> DeleteVehicleTypeAsync(Guid id);
        Task<Return<GetInformationVehicleCreateResDto>> CreateCustomerVehicleAsync(CreateCustomerVehicleReqDto reqDto);
        Task<Return<IEnumerable<GetVehicleTypeByCustomerResDto>>> GetListVehicleTypeByCustomer();
        Task<Return<dynamic>> DeleteVehicleByCustomerAsync(Guid vehicleId);
        Task<Return<dynamic>> UpdateVehicleInformationAsync(UpdateCustomerVehicleReqDto req);
        Task<Return<dynamic>> DisableVehicleTypeAsync(Guid id);
    }
}
