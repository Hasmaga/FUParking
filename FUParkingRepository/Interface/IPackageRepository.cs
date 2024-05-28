using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IPackageRepository
    {
        Task<Return<Package>> CreatePackageAsync(Package package);
        Task<Return<Package?>> GetPackageByPackageIdAsync(Guid id);
        Task<Return<List<Package>>> GetPackagesByStatusAsync(bool active);
        Task<Return<List<Package>>> GetAllPackagesAsync();
        Task<Return<IEnumerable<Package>>> GetCoinPackages(string? status);
    }
}
