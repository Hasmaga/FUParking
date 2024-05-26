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
