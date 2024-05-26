using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository;
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

        public async Task<Return<bool>> DeleteCoinPackage(Guid packageId)
        {
            try
            {
                // Check token validity
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                // Get the logged-in user
                var userLoggedResponse = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userLoggedResponse.Data == null || !userLoggedResponse.IsSuccess)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                var userLogged = userLoggedResponse.Data;
                if (!Auth.AuthManager.Contains(userLogged.Role?.Name ?? ""))
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                // Check if the package exists
                var packageResponse = await _packageRepository.GetPackageByPackageIdAsync(packageId);
                if (packageResponse.Data == null || !packageResponse.IsSuccess)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.PACKAGE_NOT_EXIST
                    };
                }

                // Update package details
                var package = packageResponse.Data;
                package.PackageStatus = StatusPackageEnum.INACTIVE;
                package.DeletedDate = DateTime.Now;
                
                var updateResponse = await _packageRepository.UpdateCoinPackage(package);
                return updateResponse;
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
