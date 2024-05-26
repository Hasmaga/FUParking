using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPackageService
    {
        Task<Return<List<Package>>> GetAvailablePackageAsync();
        Task<Return<bool>> CreateCoinPackage(CreateCoinPackageReqDto reqDto);
    }
}
