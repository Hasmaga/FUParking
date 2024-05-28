﻿using FUParkingModel.DatabaseContext;
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

        public async Task<Return<Package?>> GetPackageByPackageIdAsync(Guid id)
        {
            Return<Package?> res = new()
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.PACKAGE_NOT_EXIST,
            };
            try
            {
                Package? package = await _db.Packages.FirstOrDefaultAsync(p => p.Id.Equals(id)) ?? throw new KeyNotFoundException();
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
        
        public async Task<Return<IEnumerable<Package>>> GetCoinPackages(string? status)
        {
            try
            {
                List<Package> packages;

                if (string.IsNullOrEmpty(status))
                {
                    packages = await _db.Packages.ToListAsync();
                }
                else
                {
                    packages = await _db.Packages
                                        .Where(p => p.PackageStatus.Equals(status))
                                        .ToListAsync();
                }

                return new Return<IEnumerable<Package>>
                {
                    Data = packages,
                    IsSuccess = true,
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
