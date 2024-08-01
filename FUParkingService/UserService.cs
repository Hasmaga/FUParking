using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
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
    }
}
