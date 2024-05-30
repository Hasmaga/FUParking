using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public UserRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<User>> CreateUserAsync(User user)
        {
            try
            {
                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();
                return new Return<User>
                {
                    Data = user,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<User>
                {
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<IEnumerable<User>>> GetAllUsersAsync()
        {
            try
            {
                return new Return<IEnumerable<User>>
                {
                    Data = await _db.Users.Include(r => r.Role).ToListAsync(),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<User>>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<User>> GetUserByEmailAsync(string email)
        {
            try
            {
                return new Return<User>
                {
                    Data = await _db.Users.Include(r => r.Role).FirstOrDefaultAsync(u => u.Email == email),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<User>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<User>> GetUserByIdAsync(Guid id)
        {
            try
            {
                return new Return<User>
                {
                    Data = await _db.Users.Include(r => r.Role).FirstOrDefaultAsync(r => r.Id.Equals(id)),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<User>
                {
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<User>> UpdateUserAsync(User user)
        {
            try
            {
                _db.Users.Update(user);
                await _db.SaveChangesAsync();
                return new Return<User>
                {
                    Data = user,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<User>
                {
                    Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }
    }
}
