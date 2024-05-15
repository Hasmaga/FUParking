using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.Linq;
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
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false                        
                    };
                }
                if (!Auth.AuthManager.Contains(accountLogin.Data.Role?.Name ?? ""))
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check the email is already registered
                var isUserRegistered = await _userRepository.GetUserByEmailAsync(req.Email);
                if (isUserRegistered.IsSuccess == true && isUserRegistered.Data != null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.EMAIL_IS_EXIST,
                        IsSuccess = false
                    };
                }
                var roleStaff = await _roleRepository.GetRoleByNameAsync(RoleEnum.STAFF);
                if (roleStaff.IsSuccess == false || roleStaff.Data == null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = roleStaff.InternalErrorMessage
                    };
                }
                var user = new CreateUserPrimaryReqDto
                {
                    Email = req.Email,
                    FullName = req.FullName,
                    Password = req.Password,
                    RoleId = roleStaff.Data.Id
                };

                var result = await CreateUserAsync(user);
                if (result.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    Data = true,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<bool>> CreateSupervisorAsync(CreateUserReqDto req)
        {
            try
            {
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = accountLogin.InternalErrorMessage
                    };
                }
                if (!Auth.AuthManager.Contains(accountLogin.Data.Role?.Name ?? ""))
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check the email is already registered
                var isUserRegistered = await _userRepository.GetUserByEmailAsync(req.Email);
                if (isUserRegistered.IsSuccess == true && isUserRegistered.Data != null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.EMAIL_IS_EXIST,
                        IsSuccess = false
                    };
                }
                var roleStaff = await _roleRepository.GetRoleByNameAsync(RoleEnum.STAFF);
                if (roleStaff.IsSuccess == false || roleStaff.Data == null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = roleStaff.InternalErrorMessage
                    };
                }
                var user = new CreateUserPrimaryReqDto
                {
                    Email = req.Email,
                    FullName = req.FullName,
                    Password = req.Password,
                    RoleId = roleStaff.Data.Id
                };

                var result = await CreateUserAsync(user);
                if (result.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    Data = true,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<bool>> CreateManagerAsync(CreateUserReqDto req)
        {
            try
            {
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = accountLogin.InternalErrorMessage
                    };
                }
                if (!Auth.AuthManager.Contains(accountLogin.Data.Role?.Name ?? ""))
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check the email is already registered
                var isUserRegistered = await _userRepository.GetUserByEmailAsync(req.Email);
                if (isUserRegistered.IsSuccess == true && isUserRegistered.Data != null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.EMAIL_IS_EXIST,
                        IsSuccess = false
                    };
                }
                var roleStaff = await _roleRepository.GetRoleByNameAsync(RoleEnum.STAFF);
                if (roleStaff.IsSuccess == false || roleStaff.Data == null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = roleStaff.InternalErrorMessage
                    };
                }
                var user = new CreateUserPrimaryReqDto
                {
                    Email = req.Email,
                    FullName = req.FullName,
                    Password = req.Password,
                    RoleId = roleStaff.Data.Id
                };

                var result = await CreateUserAsync(user);
                if (result.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    Data = true,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
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
                    StatusUser = StatusUserEnum.ACTIVE
                };
                var result = await _userRepository.CreateUserAsync(newUser);
                if (result.IsSuccess == false || result.Data == null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    Data = true,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
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
