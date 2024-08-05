using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IHelpperService _helpperService;

        public PackageService(IPackageRepository packageRepository, IHelpperService helpperService)
        {
            _packageRepository = packageRepository;
            _helpperService = helpperService;
        }

        public async Task<Return<IEnumerable<dynamic>>> GetCoinPackages(string? status, int pageSize, int pageIndex)
        {
            try
            {
                if (status == StatusPackageEnum.ACTIVE || status == null)
                {
                    // Token is invalid, treat as customer and return active packages
                    var activePackagesResult = await _packageRepository.GetCoinPackages(StatusPackageEnum.ACTIVE, pageSize, pageIndex);
                    if (!activePackagesResult.IsSuccess)
                    {
                        return new Return<IEnumerable<dynamic>>
                        {
                            InternalErrorMessage = activePackagesResult.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                    return new Return<IEnumerable<dynamic>>
                    {
                        IsSuccess = true,
                        Data = activePackagesResult.Data?.Select(package => new CustomerGetCoinPackageResDto
                        {
                            Id = package.Id,
                            Name = package.Name,
                            CoinAmount = package.CoinAmount.ToString(),
                            Price = package.Price,
                            ExtraCoin = package.ExtraCoin,
                            EXPPackage = package.EXPPackage
                        }),
                        TotalRecord = activePackagesResult.TotalRecord,
                        Message = activePackagesResult.TotalRecord > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                    };
                }

                // Check if token is valid
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<dynamic>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var allPackagesResult = await _packageRepository.GetCoinPackages(null, pageSize, pageIndex);
                if (!allPackagesResult.IsSuccess)
                {
                    return new Return<IEnumerable<dynamic>>
                    {
                        InternalErrorMessage = allPackagesResult.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<IEnumerable<dynamic>>
                {
                    IsSuccess = allPackagesResult.IsSuccess,
                    Data = allPackagesResult.Data?.Select(package => new SupervisorGetCoinPackageResDto
                    {
                        Id = package.Id,
                        Name = package.Name,
                        CoinAmount = package.CoinAmount.ToString(),
                        Price = package.Price,
                        ExtraCoin = package.ExtraCoin,
                        EXPPackage = package.EXPPackage,
                        PackageStatus = package.PackageStatus,
                        CreateDate = package.CreatedDate.ToString("dd/MM/yyyy"),
                        DeletedDate = package.DeletedDate?.ToString("dd/MM/yyyy")
                    }),
                    TotalRecord = allPackagesResult.TotalRecord,
                    Message = allPackagesResult.IsSuccess ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.SERVER_ERROR
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<dynamic>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> CreateCoinPackage(CreateCoinPackageReqDto reqDto)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var isExistPackage = await _packageRepository.GetPackageByNameAsync(reqDto.Name);
                if (!isExistPackage.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.OBJECT_EXISTED, InternalErrorMessage = isExistPackage.InternalErrorMessage };
                }

                // Create package
                Package package = new()
                {
                    Name = reqDto.Name,
                    Price = reqDto.Price,
                    CoinAmount = reqDto.CoinAmount,
                    ExtraCoin = reqDto.ExtraCoin,
                    EXPPackage = reqDto.EXPPackage,
                    PackageStatus = StatusPackageEnum.ACTIVE,
                    CreatedById = checkAuth.Data.Id,
                };

                var res = await _packageRepository.CreatePackageAsync(package);
                if (!res.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = res.InternalErrorMessage };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> UpdateCoinPackage(UpdateCoinPackageReqDto updateCoinPackageReqDto)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check if the package exists
                var package = await _packageRepository.GetPackageByPackageIdAsync(updateCoinPackageReqDto.PackageId);
                if (package.Data == null || !package.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = package.InternalErrorMessage,
                        Message = ErrorEnumApplication.PACKAGE_NOT_EXIST
                    };
                }

                if (updateCoinPackageReqDto.Name?.Trim() is not null)
                {
                    var isNameExist = await _packageRepository.GetPackageByNameAsync(updateCoinPackageReqDto.Name);
                    if (isNameExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    {
                        return new Return<dynamic>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.OBJECT_EXISTED
                        };
                    }
                }

                package.Data.Name = updateCoinPackageReqDto.Name ?? package.Data.Name;
                if (updateCoinPackageReqDto.IsActive != null)
                {
                    if (updateCoinPackageReqDto.IsActive == true)
                    {
                        package.Data.PackageStatus = StatusPackageEnum.ACTIVE;
                    }
                    else if (updateCoinPackageReqDto.IsActive == false)
                    {
                        package.Data.PackageStatus = StatusPackageEnum.INACTIVE;
                    }
                }
                package.Data.LastModifyById = checkAuth.Data.Id;
                package.Data.LastModifyDate = DateTime.Now;
                var updateResponse = await _packageRepository.UpdateCoinPackage(package.Data);
                if (!updateResponse.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = updateResponse.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> DeleteCoinPackage(Guid packageId)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var packageResponse = await _packageRepository.GetPackageByPackageIdAsync(packageId);
                if (packageResponse.Data == null || !packageResponse.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = packageResponse.InternalErrorMessage,
                        Message = ErrorEnumApplication.PACKAGE_NOT_EXIST
                    };
                }

                packageResponse.Data.DeletedDate = DateTime.Now;
                packageResponse.Data.LastModifyById = checkAuth.Data.Id;
                packageResponse.Data.LastModifyDate = DateTime.Now;
                var isUpdate = await _packageRepository.UpdateCoinPackage(packageResponse.Data);
                if (!isUpdate.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isUpdate.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
