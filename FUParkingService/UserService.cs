using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.User;
using FUParkingModel.ReturnCommon;
using FUParkingRepository;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.Security.Cryptography;
using System.Text;

namespace FUParkingService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IHelpperService _helpperService;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository, IHelpperService helpperService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _helpperService = helpperService;
        }

        public async Task<Return<bool>> CreateStaffAsync(CreateUserReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check the email is already registered
                var isUserRegistered = await _userRepository.GetUserByEmailAsync(req.Email);
                if (!isUserRegistered.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.EMAIL_IS_EXIST,
                        InternalErrorMessage = isUserRegistered.InternalErrorMessage,
                    };
                }
                var roleStaff = await _roleRepository.GetRoleByNameAsync(RoleEnum.STAFF);
                if (!roleStaff.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || roleStaff.Data is null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                        InternalErrorMessage = roleStaff.InternalErrorMessage
                    };
                }
                var user = new CreateUserPrimaryReqDto
                {
                    Email = req.Email,
                    FullName = req.FullName,
                    Password = req.Password,
                    RoleId = roleStaff.Data.Id,
                    CreateById = checkAuth.Data.Id
                };
                var result = await CreateUserAsync(user);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<bool>> CreateSupervisorAsync(CreateUserReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check the email is already registered
                var isUserRegistered = await _userRepository.GetUserByEmailAsync(req.Email);
                if (!isUserRegistered.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.EMAIL_IS_EXIST,
                        InternalErrorMessage = isUserRegistered.InternalErrorMessage
                    };
                }
                var roleStaff = await _roleRepository.GetRoleByNameAsync(RoleEnum.SUPERVISOR);
                if (!roleStaff.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || roleStaff.Data is null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                        InternalErrorMessage = roleStaff.InternalErrorMessage
                    };
                }
                var user = new CreateUserPrimaryReqDto
                {
                    Email = req.Email,
                    FullName = req.FullName,
                    Password = req.Password,
                    RoleId = roleStaff.Data.Id,
                    CreateById = checkAuth.Data.Id
                };

                var result = await CreateUserAsync(user);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<bool>> CreateManagerAsync(CreateUserReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check the email is already registered
                var isUserRegistered = await _userRepository.GetUserByEmailAsync(req.Email);
                if (!isUserRegistered.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.EMAIL_IS_EXIST,
                        InternalErrorMessage = isUserRegistered.InternalErrorMessage
                    };
                }
                var roleStaff = await _roleRepository.GetRoleByNameAsync(RoleEnum.MANAGER);
                if (!roleStaff.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || roleStaff.Data is null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                        InternalErrorMessage = roleStaff.InternalErrorMessage
                    };
                }
                var user = new CreateUserPrimaryReqDto
                {
                    Email = req.Email,
                    FullName = req.FullName,
                    Password = req.Password,
                    RoleId = roleStaff.Data.Id,
                    CreateById = checkAuth.Data.Id
                };
                var result = await CreateUserAsync(user);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<IEnumerable<GetUserResDto>>> GetListUserAsync(GetListObjectWithFiller req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetUserResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _userRepository.GetListUserAsync(req);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetUserResDto>>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<IEnumerable<GetUserResDto>>
                {
                    IsSuccess = true,
                    Data = result.Data?.Select(t => new GetUserResDto
                    {
                        CreatedDate = t.CreatedDate,
                        Email = t.Email,
                        FullName = t.FullName,
                        Id = t.Id,
                        Role = t.Role?.Name ?? "",
                        Status= t.StatusUser
                    }),
                    TotalRecord = result.TotalRecord,
                    Message = result.Message
                };

            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetUserResDto>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<bool>> ChangeUserStatusAsync(Guid userId)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                // if checkAuth.Data.Id == userId, can not change status
                if (checkAuth.Data.Id == userId)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.CAN_NOT_CHANGE_STATUS_YOURSELF
                    };
                }
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (!user.IsSuccess || user.Data is null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                        InternalErrorMessage = user.InternalErrorMessage
                    };
                }

                // if user is manager, can not change status
                if (user.Data.Role == null || user.Data.Role.Name == RoleEnum.MANAGER)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                user.Data.StatusUser = user.Data.StatusUser == StatusUserEnum.ACTIVE ? StatusUserEnum.INACTIVE : StatusUserEnum.ACTIVE;
                user.Data.LastModifyById = user.Data.Id;
                user.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                var result = await _userRepository.UpdateUserAsync(user.Data);
                if (!result.IsSuccess)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<bool>> ResetWrongPasswordCountAsync(Guid userId)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (!user.IsSuccess || user.Data is null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                        InternalErrorMessage = user.InternalErrorMessage
                    };
                }

                user.Data.WrongPassword = 0;
                user.Data.LastModifyById = user.Data.Id;
                user.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                var result = await _userRepository.UpdateUserAsync(user.Data);
                if (!result.IsSuccess)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<bool>> UpdateUserAsync(UpdateUserReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var user = await _userRepository.GetUserByIdAsync(req.Id);
                if (!user.IsSuccess || user.Data is null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                        InternalErrorMessage = user.InternalErrorMessage
                    };
                }
                user.Data.FullName = req.FullName;
                user.Data.Email = req.Email.ToLower();
                user.Data.LastModifyById = user.Data.Id;
                user.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var result = await _userRepository.UpdateUserAsync(user.Data);
                if (!result.IsSuccess)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }
        public async Task<Return<bool>> UpdatePasswordAsync(UpdateUserPasswordReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                // Get user id from token
                var userid = _helpperService.GetAccIdFromLogged();
                var user = await _userRepository.GetUserByIdAsync(userid);
                if (!user.IsSuccess || user.Data is null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                        InternalErrorMessage = user.InternalErrorMessage
                    };
                }

                user.Data.PasswordHash = CreatePassHashAndPassSalt(req.Password, out byte[] passwordSalt);
                user.Data.PasswordSalt = Convert.ToBase64String(passwordSalt);
                user.Data.LastModifyById = user.Data.Id;
                user.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var result = await _userRepository.UpdateUserAsync(user.Data);
                if (!result.IsSuccess)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        #region Private Method
        private async Task<Return<bool>> CreateUserAsync(CreateUserPrimaryReqDto user)
        {
            try
            {
                User newUser = new()
                {
                    Email = user.Email.ToLower(),
                    FullName = user.FullName,
                    RoleId = user.RoleId,
                    PasswordHash = CreatePassHashAndPassSalt(user.Password, out byte[] passwordSalt),
                    PasswordSalt = Convert.ToBase64String(passwordSalt),
                    StatusUser = StatusUserEnum.ACTIVE,
                    CreatedById = user.CreateById,
                    WrongPassword = 0
                };
                var result = await _userRepository.CreateUserAsync(newUser);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || result.Data == null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        // Create Password Hash and Password Salt
        private static string CreatePassHashAndPassSalt(string pass, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(pass)));
        }
        #endregion

        public async Task<Return<bool>> DeleteUserByIdAsync(Guid id)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var userLoggedIn = _helpperService.GetAccIdFromLogged();

                if (userLoggedIn == id)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.CAN_NOT_DELETE_YOUR_ACCOUNT
                    };
                }


                var user = await _userRepository.GetUserByIdAsync(id);

                if (!user.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || user.Data is null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.USER_NOT_EXIST,
                        InternalErrorMessage = user.InternalErrorMessage
                    };
                }

                // if user is manager, can not change status
                if (user.Data.Role == null || user.Data.Role.Name == RoleEnum.MANAGER)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                user.Data.StatusUser = StatusUserEnum.INACTIVE;
                user.Data.DeletedDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                user.Data.LastModifyById = checkAuth.Data.Id;
                user.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var res = await _userRepository.UpdateUserAsync(user.Data);
                if (!res.IsSuccess)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = res.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }
    }
}
