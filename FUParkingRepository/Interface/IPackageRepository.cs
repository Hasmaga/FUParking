using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IPackageRepository
    {
        Task<Return<Package>> CreatePackageAsync(Package package);
        Task<Return<Package>> GetPackageByPackageIdAsync(Guid id);
        Task<Return<IEnumerable<Package>>> GetPackagesByStatusAsync(bool active);
        Task<Return<IEnumerable<Package>>> GetAllPackagesAsync();
        Task<Return<IEnumerable<Package>>> GetCoinPackages(string? status, int pageSize, int pageIndex);
        Task<Return<Package>> UpdateCoinPackage(Package package);
        Task<Return<Package>> GetPackageByNameAsync(string PacketName);        
    }
}
