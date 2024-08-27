using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
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
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<IEnumerable<User>>> GetAllUsersAsync()
        {
            try
            {
                var result = await _db.Users.Include(r => r.Role).ToListAsync();
                return new Return<IEnumerable<User>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = result.Count,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<User>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<IEnumerable<User>>> GetListUserAsync(GetListObjectWithFiller req)
        {
            try
            {
                var query = _db.Users
                    .Where(u => u.DeletedDate == null)
                    .Include(r => r.Role)
                    .AsQueryable();

                if (req.Attribute != null && req.SearchInput != null)
                {
                    switch (req.Attribute)
                    {
                        case "email":
                            query = query.Where(u => u.Email.Contains(req.SearchInput));
                            break;
                        case "name":
                            query = query.Where(u => u.FullName.Contains(req.SearchInput));
                            break;
                        default:
                            break;
                    }
                }
                var result = await query 
                    .OrderByDescending(t => t.CreatedDate)
                    .Skip((req.PageIndex - 1) * req.PageSize)
                    .Take(req.PageSize)
                    .ToListAsync();
                return new Return<IEnumerable<User>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = query.Count(),
                    Message = result.Count != 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<User>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<User>> GetUserByEmailAsync(string email)
        {
            try
            {
                var result = await _db.Users.Include(r => r.Role).FirstOrDefaultAsync(u => u.Email == email);
                return new Return<User>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<User>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<User>> GetUserByIdAsync(Guid id)
        {
            try
            {
                var result = await _db.Users.Include(r => r.Role).FirstOrDefaultAsync(u => u.Id.Equals(id) && u.DeletedDate == null);
                return new Return<User>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<User>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
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
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }
    }
}
