using FUParkingModel.DatabaseContext;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    public class PackageRepository : IPackageRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public PackageRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }
        public Task<Return<Package>> CreatePackageAsync(Package package)
        {
            throw new NotImplementedException();
        }
    }
}
