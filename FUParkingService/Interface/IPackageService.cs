using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPackageService
    {
        Task<Return<IEnumerable<SupervisorGetCoinPackageResDto>>> GetPackageForUserAsync(GetListObjectWithFiller req);
        Task<Return<IEnumerable<CustomerGetCoinPackageResDto>>> GetPackagesByCustomerAsync(GetListObjectWithPageReqDto req);
        Task<Return<dynamic>> CreateCoinPackage(CreateCoinPackageReqDto reqDto);
        Task<Return<dynamic>> UpdateCoinPackage(UpdateCoinPackageReqDto updateCoinPackageReqDto);
        Task<Return<dynamic>> DeleteCoinPackage(Guid packageId);
    }
}
