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
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<Role>()
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<IEnumerable<Role>>> GetAllRoleAsync()
        {
            try
            {
                var result = await _db.Roles.ToListAsync();
                return new Return<IEnumerable<Role>>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    TotalRecord = result.Count
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<Role>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<Role>> GetRoleByNameAsync(string roleName)
        {
            try
            {
                var result = await _db.Roles.FirstOrDefaultAsync(x => x.Name == roleName);
                return new Return<Role>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<Role>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }
    }
}
