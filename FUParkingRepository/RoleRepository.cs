using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public RoleRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Role>> CreateRoleAsync(Role role)
        {
            try
            {
                await _db.Roles.AddAsync(role);
                await _db.SaveChangesAsync();
                return new Return<Role>()
                {
                    Data = role,
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<Role>()
                {
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<IEnumerable<Role>>> GetAllRoleAsync()
        {
            try
            {
                return new Return<IEnumerable<Role>>
                {
                    Data = await _db.Roles.ToListAsync(),
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<Role>>
                {
                    ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }
    }
}
