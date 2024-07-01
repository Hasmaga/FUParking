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
        private readonly IUserRepository _userRepository;
        private readonly IHelpperService _helpperService;

        public PackageService(IPackageRepository packageRepository, IUserRepository userRepository, IHelpperService helpperService)
        {
            _packageRepository = packageRepository;
            _userRepository = userRepository;
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
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                    return new Return<IEnumerable<dynamic>>
                    {
                        IsSuccess = true,
                        Data = activePackagesResult.Data?.Select(package => new CustomerGetCoinPackageResDto
                        {
                            Name = package.Name,
                            CoinAmount = package.CoinAmount.ToString(),
                            Price = package.Price,
                            ExtraCoin = package.ExtraCoin,
                            EXPPackage = package.EXPPackage
                        }),
                        Message = activePackagesResult.IsSuccess ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.SERVER_ERROR
                    };
                }

                // Check if token is valid
                var isValidToken = _helpperService.IsTokenValid();

                if (!isValidToken)
                {
                    return new Return<IEnumerable<dynamic>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                else
                {
                    // Token is valid, check user role
                    var userLoggedResult = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                    if (userLoggedResult.Data == null || !userLoggedResult.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    {
                        return new Return<IEnumerable<dynamic>>
                        {                            
                            Message = ErrorEnumApplication.NOT_AUTHORITY
                        };
                    }
                    var userRole = userLoggedResult.Data.Role?.Name ?? "";
                    if (!Auth.AuthSupervisor.Contains(userRole))
                    {
                        return new Return<IEnumerable<dynamic>>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.NOT_AUTHORITY
                        };
                    }

                    // User is manager or supervisor, return all packages
                    var allPackagesResult = await _packageRepository.GetCoinPackages(null, pageSize, pageIndex);
                    return new Return<IEnumerable<dynamic>>
                    {
                        IsSuccess = allPackagesResult.IsSuccess,
                        Data = allPackagesResult.Data?.Select(package => new SupervisorGetCoinPackageResDto
                        {
                            Name = package.Name,
                            CoinAmount = package.CoinAmount.ToString(),
                            Price = package.Price,
                            ExtraCoin = package.ExtraCoin,
                            EXPPackage = package.EXPPackage,
                            PackageStatus = package.PackageStatus,
                            CreateDate = package.CreatedDate.ToString("dd/MM/yyyy"),
                            DeletedDate = package.DeletedDate?.ToString("dd/MM/yyyy")
                        }),
                        Message = allPackagesResult.IsSuccess ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.SERVER_ERROR
                    };
                }
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
                // check token valid
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                // Check role = Manager
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || !userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }               

                var isExistPackage = await _packageRepository.GetPackageByNameAsync(reqDto.Name);
                if (isExistPackage.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))    
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.OBJECT_EXISTED };
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
                    CreatedById = userlogged.Data.Id,            
                };

                var res = await _packageRepository.CreatePackageAsync(package);
                if (!res.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
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
                // Check token validity
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Get the logged-in user
                var userLoggedResponse = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userLoggedResponse.Data == null || !userLoggedResponse.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }                
                if (!Auth.AuthManager.Contains(userLoggedResponse.Data.Role?.Name ?? ""))
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }                
                // Check if the package exists
                var packageResponse = await _packageRepository.GetPackageByPackageIdAsync(updateCoinPackageReqDto.PackageId);
                if (packageResponse.Data == null || !packageResponse.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        IsSuccess = false,
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
                    packageResponse.Data.Name = updateCoinPackageReqDto.Name;
                }          

                if (updateCoinPackageReqDto.IsActive is not null)
                {
                    packageResponse.Data.PackageStatus = updateCoinPackageReqDto.IsActive.Value ? StatusPackageEnum.ACTIVE : StatusPackageEnum.INACTIVE;
                }
                // Update package details
                var updateResponse = await _packageRepository.UpdateCoinPackage(packageResponse.Data);
                if (!updateResponse.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {                        
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
                // Check token validity
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Get the logged-in user
                var userLoggedResponse = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userLoggedResponse.Data == null || !userLoggedResponse.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }               
                if (!Auth.AuthManager.Contains(userLoggedResponse.Data.Role?.Name ?? ""))
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                // Check if the package exists
                var packageResponse = await _packageRepository.GetPackageByPackageIdAsync(packageId);
                if (packageResponse.Data == null || !packageResponse.IsSuccess)
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.PACKAGE_NOT_EXIST
                    };
                }
                
                packageResponse.Data.DeletedDate = DateTime.Now;
                packageResponse.Data.LastModifyById = userLoggedResponse.Data.Id;
                packageResponse.Data.LastModifyDate = DateTime.Now;

                var isUpdate = await _packageRepository.UpdateCoinPackage(packageResponse.Data);
                if (!isUpdate.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
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
