using FUParkingModel.Enum;
using FUParkingModel.Object;
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
    }
}
