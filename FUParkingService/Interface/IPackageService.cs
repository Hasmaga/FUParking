using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPackageService
    {
        Task<Return<List<Package>>> GetAvailablePackageAsync();
    }
}
