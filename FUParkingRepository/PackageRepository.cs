using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class PackageRepository : IPackageRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public PackageRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<List<Package>>> GetAllPackagesAsync()
        {
            Return<List<Package>> res = new()
            {
                Message = ErrorEnumApplication.GET_OBJECT_ERROR,
            };
            try
            {
                List<Package> packages = await _db.Packages.ToListAsync();
                res.Message = SuccessfullyEnumServer.FOUND_OBJECT;
                res.IsSuccess = true;
                res.Data = packages;
                return res;
            }
            catch
            {
                return res;

            }
        }

        public async Task<Return<List<Package>>> GetPackagesByStatusAsync(bool active)
        {
            Return<List<Package>> res = new()
            {
                Message = ErrorEnumApplication.GET_OBJECT_ERROR,
            };
            try
            {
                string status = active ? StatusPackageEnum.ACTIVE: StatusPackageEnum.INACTIVE;
                List <Package> packages = await _db.Packages.Where(p => p.PackageStatus.ToLower().Equals(status.ToLower())).ToListAsync();
                res.Message = SuccessfullyEnumServer.FOUND_OBJECT;
                res.IsSuccess = true;
                res.Data = packages;
                return res;
            }catch
            {
                return res;

            }
        }

        public async Task<Return<Package?>> GetPackageByPackageIdAsync(Guid id)
        {
            Return<Package?> res = new()
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.PACKAGE_NOT_EXIST,
            };
            try
            {
                Package? package = await _db.Packages.FirstOrDefaultAsync(p => p.Id.Equals(id));
                if (package == null)
                {
                    throw new KeyNotFoundException();
                }
                res.IsSuccess = package != null;
                res.Data = package;
                res.Message = SuccessfullyEnumServer.SUCCESSFULLY;
                return res;
            }
            catch (Exception ex)
            {
                if (ex is KeyNotFoundException)
                {
                    return res;
                }
                res.InternalErrorMessage = ex.Message;
                res.Message = ErrorEnumApplication.SERVER_ERROR;
                return res;
            }
        }

        public async Task<Return<Package>> CreatePackageAsync(Package package)
        {
            try
            {
                await _db.Packages.AddAsync(package);
                await _db.SaveChangesAsync();
                return new Return<Package>
                {
                    Data = package,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Package>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
