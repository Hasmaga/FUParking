using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }
    }
}
