using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingModel.ReturnObject;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;

namespace FUParkingService
{
    public class AuthService : IAuthService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerTypeRepository _customerTypeRepository;
        private readonly IConfiguration _configuration;
        private readonly IWalletRepository _walletRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHelpperService _helpperService;
        private readonly IRoleRepository _roleRepository;

        public AuthService(ICustomerRepository customerRepository, ICustomerTypeRepository customerTypeRepository, IConfiguration configuration, IWalletRepository walletRepository, IUserRepository userRepository, IHelpperService helpperService, IRoleRepository roleRepository)
        {
            _customerRepository = customerRepository;
            _customerTypeRepository = customerTypeRepository;
            _configuration = configuration;
            _walletRepository = walletRepository;
            _userRepository = userRepository;
            _helpperService = helpperService;
            _roleRepository = roleRepository;
        }

        public async Task<Return<LoginResDto>> LoginWithGoogleAsync(GoogleReturnAuthenticationResDto login)
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                if (login.IsAuthentication == false)
                {
                    return new Return<LoginResDto>
                    {
                        ErrorMessage = ErrorEnumApplication.GOOGLE_LOGIN_FAILED,
                        IsSuccess = false,
                    };
                }
                // Check the email is @fpt.edu.vn only
                if (!login.Email.Contains("@fpt.edu.vn"))
                {
                    return new Return<LoginResDto>
                    {
                        ErrorMessage = ErrorEnumApplication.NOT_EMAIL_FPT_UNIVERSITY,
                        IsSuccess = false,
                    };
                }
                // Check if the user is already registered
                var isUserRegistered = await _customerRepository.GetCustomerByEmail(login.Email);
                // If the user is not registered, create a new user
                if (isUserRegistered.Data == null)
                {
                    // Get the customer type
                    var customerType = await _customerTypeRepository.GetCustomerTypeByNameAsync(CustomerTypeEnum.PAID);
                    if (customerType.Data == null)
                    {
                        return new Return<LoginResDto>
                        {
                            ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR,
                            IsSuccess = false,
                        };
                    }
                    var newCustomer = new Customer
                    {
                        FullName = login.Name,
                        Email = login.Email,
                        CustomerTypeId = customerType.Data.Id,
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    };
                    var result = await _customerRepository.CreateNewCustomerAsync(newCustomer);
                    if (result.IsSuccess == false || result.Data == null)
                    {
                        transaction.Dispose();
                        return new Return<LoginResDto>
                        {
                            ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = result.InternalErrorMessage
                        };
                    }
                    // Create waller for the new customer
                    Wallet cusWallet = new()
                    {
                        CustomerId = result.Data.Id,
                        Balance = 0,
                        WalletStatus = StatusWalletEnum.ACTIVE
                    };
                    var resultWallet = await _walletRepository.CreateWalletAsync(cusWallet);
                    if (resultWallet.IsSuccess == false || resultWallet.Data == null)
                    {
                        transaction.Dispose();
                        return new Return<LoginResDto>
                        {
                            ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = resultWallet.InternalErrorMessage
                        };
                    }
                    transaction.Complete();
                    if (result.IsSuccess == true && result.Data != null && resultWallet.IsSuccess == true && resultWallet.Data != null)
                    {
                        return new Return<LoginResDto>
                        {
                            Data = new LoginResDto
                            {
                                BearerToken = CreateBearerTokenAccount(result.Data.Id),
                                Name = result.Data.FullName,
                                Email = result.Data.Email
                            },
                            IsSuccess = true,
                            SuccessfullyMessage = SuccessfullyEnumServer.SUCCESSFULLY
                        };
                    }
                    else
                    {
                        return new Return<LoginResDto>
                        {
                            ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = resultWallet.InternalErrorMessage + " " + result.InternalErrorMessage
                        };
                    }
                }
                if (isUserRegistered != null && isUserRegistered.Data != null)
                {
                    return new Return<LoginResDto>
                    {
                        Data = new LoginResDto
                        {
                            BearerToken = CreateBearerTokenAccount(isUserRegistered.Data.Id),
                            Name = isUserRegistered.Data.FullName,
                            Email = isUserRegistered.Data.Email
                        },
                        IsSuccess = true,
                        SuccessfullyMessage = SuccessfullyEnumServer.SUCCESSFULLY
                    };
                }
                transaction.Dispose();
                return new Return<LoginResDto>
                {
                    ErrorMessage = ErrorEnumApplication.GOOGLE_LOGIN_FAILED,
                    IsSuccess = false,
                };
            }
            catch (Exception ex)
            {
                transaction.Dispose();
                return new Return<LoginResDto>
                {
                    ErrorMessage = ErrorEnumApplication.GOOGLE_LOGIN_FAILED,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<bool>> CreateStaffAsync(CreateUserReqDto req)
        {
            try
            {
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<bool>
                    {
                        ErrorMessage = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());                
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                {
                    return new Return<bool>
                    {
                        ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = accountLogin.InternalErrorMessage
                    };
                }
                if ((accountLogin.Data.Role ?? new Role()).Name is not RoleEnum.MANAGER and not RoleEnum.SUPERVISOR)
                {
                    return new Return<bool>
                    {
                        ErrorMessage = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check the email is already registered
                var isUserRegistered = await _userRepository.GetUserByEmailAsync(req.Email);
                if (isUserRegistered.IsSuccess == true && isUserRegistered.Data != null)
                {
                    return new Return<bool>
                    {
                        ErrorMessage = ErrorEnumApplication.EMAIL_IS_EXIST,
                        IsSuccess = false
                    };
                }
                var roleStaff = await _roleRepository.GetRoleByNameAsync(RoleEnum.STAFF);
                if (roleStaff.IsSuccess == false || roleStaff.Data == null)
                {
                    return new Return<bool>
                    {
                        ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR,
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
                        ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    Data = true,
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
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
                        ErrorMessage = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                {
                    return new Return<bool>
                    {
                        ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = accountLogin.InternalErrorMessage
                    };
                }
                if ((accountLogin.Data.Role ?? new Role()).Name is not RoleEnum.MANAGER)
                {
                    return new Return<bool>
                    {
                        ErrorMessage = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check the email is already registered
                var isUserRegistered = await _userRepository.GetUserByEmailAsync(req.Email);
                if (isUserRegistered.IsSuccess == true && isUserRegistered.Data != null)
                {
                    return new Return<bool>
                    {
                        ErrorMessage = ErrorEnumApplication.EMAIL_IS_EXIST,
                        IsSuccess = false
                    };
                }
                var roleStaff = await _roleRepository.GetRoleByNameAsync(RoleEnum.STAFF);
                if (roleStaff.IsSuccess == false || roleStaff.Data == null)
                {
                    return new Return<bool>
                    {
                        ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR,
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
                        ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    Data = true,
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
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
                        ErrorMessage = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                {
                    return new Return<bool>
                    {
                        ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = accountLogin.InternalErrorMessage
                    };
                }
                if ((accountLogin.Data.Role ?? new Role()).Name is not RoleEnum.MANAGER)
                {
                    return new Return<bool>
                    {
                        ErrorMessage = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check the email is already registered
                var isUserRegistered = await _userRepository.GetUserByEmailAsync(req.Email);
                if (isUserRegistered.IsSuccess == true && isUserRegistered.Data != null)
                {
                    return new Return<bool>
                    {
                        ErrorMessage = ErrorEnumApplication.EMAIL_IS_EXIST,
                        IsSuccess = false
                    };
                }
                var roleStaff = await _roleRepository.GetRoleByNameAsync(RoleEnum.STAFF);
                if (roleStaff.IsSuccess == false || roleStaff.Data == null)
                {
                    return new Return<bool>
                    {
                        ErrorMessage = ErrorEnumApplication.GET_OBJECT_ERROR,
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
                        ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    Data = true,
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<LoginResDto>> LoginWithCredentialAsync(LoginWithCredentialReqDto req)
        {
            try
            {
                // Check the email is registered
                var isUserRegistered = await _userRepository.GetUserByEmailAsync(req.Email.ToLower());
                if (isUserRegistered.IsSuccess == false || isUserRegistered.Data == null)
                {
                    return new Return<LoginResDto>
                    {
                        ErrorMessage = ErrorEnumApplication.CRENEDTIAL_IS_WRONG,
                        IsSuccess = false                        
                    };
                }
                // Check the password is correct
                if (!VerifyPasswordHash(req.Password, Convert.FromBase64String(isUserRegistered.Data.PasswordSalt ?? ""), isUserRegistered.Data.PasswordHash ?? ""))
                {
                    return new Return<LoginResDto>
                    {
                        ErrorMessage = ErrorEnumApplication.CRENEDTIAL_IS_WRONG,
                        IsSuccess = false
                    };
                }
                return new Return<LoginResDto>
                {
                    Data = new LoginResDto
                    {
                        BearerToken = CreateBearerTokenAccount(isUserRegistered.Data.Id),
                        Name = isUserRegistered.Data.FullName,
                        Email = isUserRegistered.Data.Email
                    },
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.SUCCESSFULLY
                };
            } catch (Exception ex)
            {
                return new Return<LoginResDto>
                {
                    ErrorMessage = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        #region Private methods
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
                        ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    Data = true,
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
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

        // Verify Password Hash
        private static bool VerifyPasswordHash(string pass, byte[] passwordSalt, string passwordHash)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(pass)));
            return computedHash.Equals(passwordHash);
        }
        private string CreateBearerTokenAccount(Guid id)
        {
            List<Claim> claims =
            [
                new Claim(ClaimTypes.Sid, id.ToString()),
            ];
            var securityKey = _configuration.GetSection("AppSettings:Token").Value ?? throw new Exception(ErrorEnumApplication.SERVER_ERROR);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return token == null ? throw new Exception(ErrorEnumApplication.SERVER_ERROR) : tokenHandler.WriteToken(token);
        }                
        #endregion
    }
}
