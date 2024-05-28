using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;

using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.Collections.Generic;

namespace FUParkingService
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IHelpperService _helpperService;
        private readonly IUserRepository _userRepository;

        public PackageService(IPackageRepository packageRepository, IHelpperService helpperService, IUserRepository userRepository)
        {
            _packageRepository = packageRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
        }

        public async Task<Return<IEnumerable<dynamic>>> GetCoinPackages(string? status)
        {
            try
            {
                if (status == StatusPackageEnum.ACTIVE)
                {
                    // Token is invalid, treat as customer and return active packages
                    var activePackagesResult = await _packageRepository.GetCoinPackages(StatusPackageEnum.ACTIVE);

                    return new Return<IEnumerable<dynamic>>
                    {
                        IsSuccess = activePackagesResult.IsSuccess,
                        Data = activePackagesResult.Data?.Select(package => new CustomerGetCoinPackageResDto
                        {
                            Name = package.Name,
                            CoinAmount = package.CoinAmount.ToString(),
                            Price = package.Price
                        }),
                        Message = activePackagesResult.IsSuccess ? SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY : ErrorEnumApplication.SERVER_ERROR
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
                    if (userLoggedResult.Data == null || !userLoggedResult.IsSuccess)
                    {
                        return new Return<IEnumerable<dynamic>>
                        {
                            IsSuccess = false,
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
                    var allPackagesResult = await _packageRepository.GetCoinPackages(null);
                    return new Return<IEnumerable<dynamic>>
                    {
                        IsSuccess = allPackagesResult.IsSuccess,
                        Data = allPackagesResult.Data?.Select(package => new SupervisorGetCoinPackageResDto
                        {
                            Name = package.Name,
                            CoinAmount = package.CoinAmount.ToString(),
                            Price = package.Price,
                            PackageStatus = package.PackageStatus,
                            CreateDate = package.CreatedDate.ToString("dd/MM/yyyy")
                        }),
                        Message = allPackagesResult.IsSuccess ? SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY : ErrorEnumApplication.SERVER_ERROR
                    };
                }
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<dynamic>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
        
        public async Task<Return<List<Package>>> GetAvailablePackageAsync()
        {
            Return<List<Package>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                Return<List<Package>> packagesRes = await _packageRepository.GetPackagesByStatusAsync(true);
                if(!packagesRes.IsSuccess) {
                    return res;
                }
                return packagesRes;
            }
            catch
            {
                throw;
            }
        }
        
        public async Task<Return<bool>> CreateCoinPackage(CreateCoinPackageReqDto reqDto)
        {
            try
            {
                // check token valid
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                // Check role = Manager
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check package's name exist
                var existPackages = await _packageRepository.GetAllPackagesAsync();
                if (existPackages.Data != null && existPackages.IsSuccess == true)
                {
                    bool isExistPackageName = existPackages.Data.Exists(p => p.Name.ToLower().Equals(reqDto.Name.ToLower()));
                    if (isExistPackageName)
                    {
                        return new Return<bool>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.OBJECT_EXISTED
                        };
                    }
                }

                // Create package
                Package package = new()
                {
                    Name = reqDto.Name,
                    Price = reqDto.Price,
                    CoinAmount = reqDto.CoinAmount,
                    PackageStatus = StatusPackageEnum.ACTIVE,
                    CreatedDate = DateTime.Now,
                };

                var res = await _packageRepository.CreatePackageAsync(package);
                return new Return<bool>
                {
                    Data = res.IsSuccess,
                    IsSuccess = res.IsSuccess,
                    Message = res.IsSuccess ? SuccessfullyEnumServer.SUCCESSFULLY : ErrorEnumApplication.SERVER_ERROR,
                };
            }
            catch (Exception)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }    
    }
}
