using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IRoleRepository
    {
        Task<Return<Role>> CreateRoleAsync(Role role);
        Task<Return<IEnumerable<Role>>> GetAllRoleAsync();
    }
}
