using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IPackageRepository
    {
        Task<Return<Package>> CreatePackageAsync(Package package);
    }
}
