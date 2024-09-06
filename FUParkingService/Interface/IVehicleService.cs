using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Vehicle;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ResponseObject.Vehicle;
using FUParkingModel.ResponseObject.VehicleType;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IVehicleService
    {
        Task<Return<IEnumerable<GetVehicleTypeByUserResDto>>> GetVehicleTypesAsync(GetListObjectWithFiller req);
        Task<Return<bool>> CreateVehicleTypeAsync(CreateVehicleTypeReqDto reqDto);
        Task<Return<bool>> UpdateVehicleTypeAsync(UpdateVehicleTypeReqDto reqDto);
        Task<Return<IEnumerable<GetVehicleForUserResDto>>> GetVehiclesAsync(GetListObjectWithFillerAttributeAndDateReqDto req);
        Task<Return<IEnumerable<GetCustomerVehicleByCustomerResDto>>> GetListCustomerVehicleByCustomerIdAsync();
        Task<Return<dynamic>> DeleteVehicleTypeAsync(Guid id);
        Task<Return<GetInformationVehicleCreateResDto>> CreateCustomerVehicleAsync(CreateCustomerVehicleReqDto reqDto);
        Task<Return<IEnumerable<GetVehicleTypeByCustomerResDto>>> GetListVehicleTypeByCustomer();
        Task<Return<dynamic>> DeleteVehicleByCustomerAsync(Guid vehicleId);
        Task<Return<dynamic>> UpdateVehicleInformationAsync(UpdateCustomerVehicleReqDto req);
        Task<Return<dynamic>> ChangeStatusVehicleTypeAsync(Guid id, bool isActive);
        Task<Return<dynamic>> ChangeStatusVehicleByUserAsync(UpdateNewCustomerVehicleByUseReqDto req);
        Task<Return<dynamic>> UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync(UpdateStatusInactiveAndActiveCustomerVehicleByUserReqDto req);
        Task<Return<GetCustomerVehicleByCustomerResDto>> GetCustomerVehicleByVehicleIdAsync(Guid VehicleId);
        Task<Return<dynamic>> UpdateVehicleInformationByUserAsync(Guid vehicleId, Guid vehicleTypeId);
        Task<Return<StatisticVehicleResDto>> GetStatisticVehicleAsync();
        Task<Return<IEnumerable<GetVehicleForUserResDto>>> GetListVehicleByCustomerIdForUserAsync(Guid customerId);
        Task<Return<dynamic>> CreateListVehicleForCustomerByUserAsync(CreateListVehicleForCustomerByUserReqDto req);
        Task<Return<dynamic>> DeleteVehicleByUserAsync(Guid vehicleId);
    }
}
