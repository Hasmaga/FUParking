using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPackageService
    {
        Task<Return<IEnumerable<dynamic>>> GetCoinPackages(string? status, int pageSize, int pageIndex);        
        Task<Return<dynamic>> CreateCoinPackage(CreateCoinPackageReqDto reqDto);
        Task<Return<dynamic>> UpdateCoinPackage(UpdateCoinPackageReqDto updateCoinPackageReqDto);
        Task<Return<dynamic>> DeleteCoinPackage(Guid packageId);
    }
}
