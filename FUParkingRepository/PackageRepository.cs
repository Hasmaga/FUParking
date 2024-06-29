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

        public async Task<Return<IEnumerable<Package>>> GetAllPackagesAsync()
        {
            Return<IEnumerable<Package>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                var result = await _db.Packages.Where(t => t.DeletedDate == null).OrderByDescending(t => t.CreatedDate).ToListAsync();
                res.Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT;
                res.IsSuccess = true;
                res.TotalRecord = result.Count;
                res.Data = result;
                return res;
            }
            catch (Exception e)
            {
                res.InternalErrorMessage = e;
                return res;
            }
        }

        public async Task<Return<IEnumerable<Package>>> GetPackagesByStatusAsync(bool active)
        {
            Return<IEnumerable<Package>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                string status = active ? StatusPackageEnum.ACTIVE : StatusPackageEnum.INACTIVE;
                var result = await _db.Packages.Where(p => p.PackageStatus != null && p.PackageStatus.ToLower().Equals(status.ToLower())).ToListAsync();
                res.Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT;
                res.IsSuccess = true;
                res.TotalRecord = result.Count;
                res.Data = result;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
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
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
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
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<Package>>> GetCoinPackages(string? status, int pageSize, int pageIndex)
        {
            try
            {
                var query = _db.Packages.Where(p => p.DeletedDate == null).AsQueryable();
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(p => p.PackageStatus == status);
                }
                var result = await query
                                .OrderByDescending(t => t.CreatedDate)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();
                return new Return<IEnumerable<Package>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = result.Count,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Package>>
                {                    
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Package>> UpdateCoinPackage(Package package)
        {
            Return<Package> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                _db.Packages.Update(package);
                await _db.SaveChangesAsync();
                res.IsSuccess = true;
                res.Data = package;
                res.Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY;
                return res;
            }
            catch(Exception e)
            {
                res.InternalErrorMessage = e;
                return res;
            }
        }

        public async Task<Return<Package>> GetPackageByNameAsync(string PacketName)
        {
            try
            {
                var result = await _db.Packages.Where(p => p.DeletedDate == null).FirstOrDefaultAsync(p => p.Name.ToUpper().Equals(PacketName.ToUpper()));
                return new Return<Package>
                {
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Return<Package>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }
    }
}
