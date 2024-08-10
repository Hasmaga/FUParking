using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IPackageRepository
    {
        Task<Return<Package>> CreatePackageAsync(Package package);
        Task<Return<Package>> GetPackageByPackageIdAsync(Guid id);
        Task<Return<IEnumerable<Package>>> GetPackageForCustomerAsync(GetListObjectWithPageReqDto req);
        Task<Return<IEnumerable<Package>>> GetAllPackagesAsync(GetListObjectWithFiller req);        
        Task<Return<Package>> UpdateCoinPackage(Package package);
        Task<Return<Package>> GetPackageByNameAsync(string PacketName);
    }
}
