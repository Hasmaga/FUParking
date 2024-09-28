using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
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

        public async Task<Return<IEnumerable<Package>>> GetAllPackagesAsync(GetListObjectWithFiller req)
        {
            Return<IEnumerable<Package>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                var query = _db.Packages
                    .Where(t => t.DeletedDate == null)
                    .Include(t => t.CreateBy)
                    .Include(t => t.LastModifyBy)
                    .OrderByDescending(t => t.CreatedDate)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(req.Attribute) && !string.IsNullOrEmpty(req.SearchInput))
                {
                    switch (req.Attribute.ToUpper())
                    {
                        case "NAME":
                            query = query.Where(r => r.Name.Contains(req.SearchInput));
                            break;
                        case "STATUS":
                            query = query.Where(r => r.PackageStatus.Contains(req.SearchInput));
                            break;
                        default:
                            break;
                    }
                }
                var result = await query
                    .OrderByDescending(t => t.CreatedDate)
                    .Skip((req.PageIndex - 1) * req.PageSize)
                    .Take(req.PageSize)
                    .ToListAsync();

                res.Message = query.Any() ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT;
                res.IsSuccess = true;
                res.TotalRecord = query.Count();
                res.Data = result;
                return res;
            }
            catch (Exception e)
            {
                res.InternalErrorMessage = e;
                return res;
            }
        }

        public async Task<Return<IEnumerable<Package>>> GetPackageForCustomerAsync(GetListObjectWithPageReqDto req)
        {
            try
            {
                var query = _db.Packages
                    .Where(p => p.DeletedDate == null && p.PackageStatus.Equals(StatusPackageEnum.ACTIVE))
                    .AsQueryable();

                var result = await query
                    .OrderByDescending(t => t.CreatedDate)
                    .Skip((req.PageIndex - 1) * req.PageSize)
                    .Take(req.PageSize)
                    .ToListAsync();

                return new Return<IEnumerable<Package>>
                {
                    Data = result,
                    TotalRecord = query.Count(),
                    Message = query.Any() ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = true
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

        public async Task<Return<IEnumerable<Package>>> GetPackagesByStatusAsync(bool active)
        {
            Return<IEnumerable<Package>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                string status = active ? StatusPackageEnum.ACTIVE : StatusPackageEnum.INACTIVE;
                var result = await _db.Packages.Where(p => p.PackageStatus != null && p.PackageStatus.ToLower().Equals(status.ToLower())&& p.DeletedDate == null).ToListAsync();
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
            catch (Exception e)
            {
                res.InternalErrorMessage = e;
                return res;
            }
        }

        public async Task<Return<Package>> GetPackageByNameAsync(string PacketName)
        {
            try
            {
                var result = await _db.Packages
                    .Where(p => p.DeletedDate == null)
                    .FirstOrDefaultAsync(p => p.Name.ToUpper().Equals(PacketName.ToUpper()));
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
