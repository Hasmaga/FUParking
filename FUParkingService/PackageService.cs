using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.Collections.Generic;

namespace FUParkingService
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepository;

        public PackageService(IPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
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


    }
}
