using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPackageService
    {
        Task<Return<IEnumerable<dynamic>>> GetCoinPackages(string? status);
        Task<Return<List<Package>>> GetAvailablePackageAsync();
        Task<Return<bool>> CreateCoinPackage(CreateCoinPackageReqDto reqDto);
        Task<Return<bool>> UpdateCoinPackage(Guid packageId, UpdateCoinPackageReqDto updateCoinPackageReqDto);
    }
}
