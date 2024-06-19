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
                List<Package> packages = await _db.Packages.OrderByDescending(t => t.CreatedDate).ToListAsync();
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
                string status = active ? StatusPackageEnum.ACTIVE : StatusPackageEnum.INACTIVE;
                List<Package> packages = await _db.Packages.Where(p => p.PackageStatus != null && p.PackageStatus.ToLower().Equals(status.ToLower())).ToListAsync();
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

        public async Task<Return<Package>> GetPackageByPackageIdAsync(Guid id)
        {
            try
            {
                var result = await _db.Packages.Where(p => p.DeletedDate == null).FirstOrDefaultAsync(p => p.Id.Equals(id));
                return new Return<Package>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<Package>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = ex.Message
                };
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

        public async Task<Return<IEnumerable<Package>>> GetCoinPackages(string? status, int pageSize, int pageIndex)
        {
            try
            {
                List<Package> packages;
                int totalRecord = 0;

                if (string.IsNullOrEmpty(status))
                {
                    packages = await _db.Packages
                        .OrderByDescending(t => t.CreatedDate)
                        .Skip(pageSize * (pageIndex - 1))
                        .Take(pageSize)
                        .ToListAsync();

                    totalRecord = await _db.Packages.CountAsync();
                }
                else
                {
                    packages = await _db.Packages
                                        .Where(p => p.PackageStatus.Equals(status))
                                        .OrderByDescending(t => t.CreatedDate)
                                        .Skip(pageSize * (pageIndex - 1))
                                        .Take(pageSize)
                                        .ToListAsync();

                    totalRecord = await _db.Packages
                                        .Where(p => p.PackageStatus.Equals(status))
                                        .CountAsync();
                }

                return new Return<IEnumerable<Package>>
                {
                    Data = packages,
                    IsSuccess = true,
                    TotalRecord = totalRecord,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Package>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<bool>> UpdateCoinPackage(Package package)
        {
            Return<bool> res = new()
            {
                Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR,
            };
            try
            {
                _db.Packages.Update(package);
                await _db.SaveChangesAsync();
                res.IsSuccess = true;
                res.Data = true;
                res.Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY;
                return res;
            }
            catch
            {
                return res;
            }
        }
    }
}
