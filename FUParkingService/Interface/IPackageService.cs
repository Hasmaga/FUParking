using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPackageService
    {
        Task<Return<IEnumerable<dynamic>>> GetCoinPackages(string? status);
    }
}
