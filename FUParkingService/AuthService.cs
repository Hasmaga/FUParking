using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ResponseObject.Customer;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace FUParkingService
{
    public class AuthService : IAuthService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IConfiguration _configuration;
        private readonly IWalletRepository _walletRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHelpperService _helpperService;

        public AuthService(ICustomerRepository customerRepository, IConfiguration configuration, IWalletRepository walletRepository, IUserRepository userRepository, IHelpperService helpperService)
        {
            _customerRepository = customerRepository;
            _configuration = configuration;
            _walletRepository = walletRepository;
            _userRepository = userRepository;
            _helpperService = helpperService;
        }

        public async Task<Return<LoginResDto>> LoginWithGoogleAsync(GoogleReturnAuthenticationResDto login)
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                if (login.IsAuthentication == false)
                {
                    return new Return<LoginResDto> { Message = ErrorEnumApplication.GOOGLE_LOGIN_FAILED };
                }
                // Check the email is @fpt.edu.vn only
                if (!login.Email.Contains("@fpt.edu.vn"))
                {
                    return new Return<LoginResDto> { Message = ErrorEnumApplication.NOT_EMAIL_FPT_UNIVERSITY };
                }
                // Check if the user is already registered
                var isUserRegistered = await _customerRepository.GetCustomerByEmailAsync(login.Email);
                // If the user is not registered, create a new user
                if (isUserRegistered.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) && isUserRegistered.Data is not null)
                {
                    return new Return<LoginResDto>
                    {
                        Data = new LoginResDto
                        {
                            BearerToken = CreateBearerTokenAccount(isUserRegistered.Data.Id),
                            Name = isUserRegistered.Data.FullName ?? "",
                            Email = isUserRegistered.Data.Email ?? ""
                        },
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.LOGIN_SUCCESSFULLY
                    };
                }
                else if (isUserRegistered.Message.Equals(ErrorEnumApplication.SERVER_ERROR))
                {
                    return new Return<LoginResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = isUserRegistered.InternalErrorMessage };
                }
                // Get the customer type
                var customerType = await _customerRepository.GetCustomerTypeByNameAsync(CustomerTypeEnum.PAID);
                if (customerType.Message.Equals(ErrorEnumApplication.SERVER_ERROR) || customerType.Data == null)
                {
                    return new Return<LoginResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customerType.InternalErrorMessage };
                }
                Customer newCustomer = new() { FullName = login.Name, Email = login.Email, CustomerTypeId = customerType.Data.Id, StatusCustomer = StatusCustomerEnum.ACTIVE };
                var result = await _customerRepository.CreateNewCustomerAsync(newCustomer);
                if (result.Message.Equals(ErrorEnumApplication.SERVER_ERROR) || result.Data == null)
                {
                    transaction.Dispose();
                    return new Return<LoginResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                }
                // Create waller for the new customer
                Wallet cusWalletMain = new() { WalletType = WalletType.MAIN, CustomerId = result.Data.Id };
                var resultWallet = await _walletRepository.CreateWalletAsync(cusWalletMain);
                if (resultWallet.Message.Equals(ErrorEnumApplication.SERVER_ERROR) || resultWallet.Data == null)
                {
                    transaction.Dispose();
                    return new Return<LoginResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = resultWallet.InternalErrorMessage };
                }
                Wallet cusWalletExtra = new() { CustomerId = result.Data.Id, WalletType = WalletType.EXTRA };
                var resultWalletExtra = await _walletRepository.CreateWalletAsync(cusWalletExtra);
                if (resultWalletExtra.Message.Equals(ErrorEnumApplication.SERVER_ERROR) || resultWalletExtra.Data == null)
                {
                    transaction.Dispose();
                    return new Return<LoginResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = resultWalletExtra.InternalErrorMessage };
                }
                transaction.Complete();
                return new Return<LoginResDto>
                {
                    Data = new LoginResDto
                    {
                        BearerToken = CreateBearerTokenAccount(result.Data.Id),
                        Name = result.Data.FullName ?? "",
                        Email = result.Data.Email ?? ""
                    },
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.LOGIN_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                transaction.Dispose();
                return new Return<LoginResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<LoginResDto>> LoginWithCredentialAsync(LoginWithCredentialReqDto req)
        {
            try
            {                
                var isUserRegistered = await _userRepository.GetUserByEmailAsync(req.Email.ToLower());                
                if (!isUserRegistered.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || isUserRegistered.Data is null)
                {
                    return new Return<LoginResDto>
                    {
                        InternalErrorMessage = isUserRegistered.InternalErrorMessage,
                        Message = ErrorEnumApplication.CRENEDTIAL_IS_WRONG
                    };
                }
                if (isUserRegistered.Data.WrongPassword >= 5)
                {
                    return new Return<LoginResDto>
                    {
                        Message = ErrorEnumApplication.ACCOUNT_IS_LOCK
                    };
                }
                if (isUserRegistered.Data.StatusUser == StatusUserEnum.INACTIVE)
                {
                    return new Return<LoginResDto>
                    {
                        Message = ErrorEnumApplication.ACCOUNT_IS_INACTIVE
                    };
                }
                // Check the password is correct
                if (!VerifyPasswordHash(req.Password, Convert.FromBase64String(isUserRegistered.Data.PasswordSalt ?? ""), isUserRegistered.Data.PasswordHash ?? ""))
                {
                    // Update the wrong password count
                    isUserRegistered.Data.WrongPassword++;
                    var result = await _userRepository.UpdateUserAsync(isUserRegistered.Data);
                    if (!result.IsSuccess)
                    {
                        return new Return<LoginResDto>
                        {
                            InternalErrorMessage = result.InternalErrorMessage,
                            Message = ErrorEnumApplication.CRENEDTIAL_IS_WRONG
                        };
                    }
                    return new Return<LoginResDto>
                    {
                        Message = ErrorEnumApplication.CRENEDTIAL_IS_WRONG
                    };
                }
                // Reset the wrong password count
                if (isUserRegistered.Data.WrongPassword > 0)
                {
                    isUserRegistered.Data.WrongPassword = 0;
                    var result = await _userRepository.UpdateUserAsync(isUserRegistered.Data);
                    if (!result.IsSuccess)
                    {
                        return new Return<LoginResDto>
                        {
                            InternalErrorMessage = result.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                }
                return new Return<LoginResDto>
                {
                    Data = new LoginResDto
                    {
                        BearerToken = CreateBearerTokenAccount(isUserRegistered.Data.Id),
                        Name = isUserRegistered.Data.FullName,
                        Email = isUserRegistered.Data.Email,
                        Role = isUserRegistered.Data.Role?.Name ?? ""
                    },
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.LOGIN_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<LoginResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<LoginResDto>> CheckRoleByTokenAsync()
        {
            try
            {
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<LoginResDto>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHENTICATION
                    };
                }
                var accountLogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!accountLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || accountLogged.Data == null || accountLogged.Data.Role?.Name == null)
                {
                    return new Return<LoginResDto>
                    {
                        InternalErrorMessage = accountLogged.InternalErrorMessage,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (accountLogged.Data.WrongPassword >= 5)
                {
                    return new Return<LoginResDto>
                    {
                        Message = ErrorEnumApplication.ACCOUNT_IS_LOCK
                    };
                }
                if (accountLogged.Data.StatusUser.Equals(StatusUserEnum.INACTIVE))
                {
                    return new Return<LoginResDto>
                    {
                        Message = ErrorEnumApplication.ACCOUNT_IS_BANNED
                    };
                }
                return new Return<LoginResDto>
                {
                    Data = new LoginResDto
                    {
                        Role = accountLogged.Data.Role?.Name ?? ""
                    },
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<LoginResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<LoginWithGoogleMoblieResDto>> LoginWithGoogleMobileAsync(string one_time_code)
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var clientId = _configuration.GetSection("Authentication:Google:ClientId").Value;
                if (string.IsNullOrEmpty(clientId))
                {
                    return new Return<LoginWithGoogleMoblieResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var settings = new ValidationSettings() { Audience = [clientId] };
                Payload payload = await ValidateAsync(one_time_code, settings);
                if (payload == null)
                {
                    return new Return<LoginWithGoogleMoblieResDto> { Message = ErrorEnumApplication.GOOGLE_LOGIN_FAILED };
                }
                if (!payload.HostedDomain.Equals("fpt.edu.vn"))
                {
                    return new Return<LoginWithGoogleMoblieResDto> { Message = ErrorEnumApplication.NOT_EMAIL_FPT_UNIVERSITY };
                }
                var isUserRegistered = await _customerRepository.GetCustomerByEmailAsync(payload.Email);
                if (isUserRegistered.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    // Create new customer
                    var customerType = await _customerRepository.GetCustomerTypeByNameAsync(CustomerTypeEnum.PAID);
                    if (customerType.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT) || customerType.Data == null)
                    {
                        return new Return<LoginWithGoogleMoblieResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customerType.InternalErrorMessage };
                    }
                    Customer newCustomer = new()
                    {
                        FullName = payload.Name,
                        Email = payload.Email,
                        CustomerTypeId = customerType.Data.Id,
                        StatusCustomer = StatusCustomerEnum.ACTIVE,
                    };
                    var resultCreateCus = await _customerRepository.CreateNewCustomerAsync(newCustomer);
                    if (resultCreateCus.Message.Equals(ErrorEnumApplication.SERVER_ERROR) || resultCreateCus.Data == null)
                    {
                        transaction.Dispose();
                        return new Return<LoginWithGoogleMoblieResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = resultCreateCus.InternalErrorMessage };
                    }
                    // Create waller for the new customer
                    Wallet cusWalletMain = new()
                    {
                        WalletType = WalletType.MAIN,
                        CustomerId = resultCreateCus.Data.Id
                    };
                    var resultWallet = await _walletRepository.CreateWalletAsync(cusWalletMain);
                    if (resultWallet.Message.Equals(ErrorEnumApplication.SERVER_ERROR) || resultWallet.Data == null)
                    {
                        transaction.Dispose();
                        return new Return<LoginWithGoogleMoblieResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = resultWallet.InternalErrorMessage };
                    }
                    Wallet cusWalletExtra = new()
                    {
                        CustomerId = resultCreateCus.Data.Id,
                        WalletType = WalletType.EXTRA
                    };
                    var resultWalletExtra = await _walletRepository.CreateWalletAsync(cusWalletExtra);
                    if (resultWalletExtra.Message.Equals(ErrorEnumApplication.SERVER_ERROR) || resultWalletExtra.Data == null)
                    {
                        transaction.Dispose();
                        return new Return<LoginWithGoogleMoblieResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = resultWalletExtra.InternalErrorMessage };
                    }
                    transaction.Complete();
                    return new Return<LoginWithGoogleMoblieResDto>
                    {
                        Data = new LoginWithGoogleMoblieResDto
                        {
                            BearerToken = CreateBearerTokenAccount(resultCreateCus.Data.Id),
                            Name = resultCreateCus.Data.FullName,
                            Email = resultCreateCus.Data.Email,
                            Avatar = payload.Picture
                        },
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.LOGIN_SUCCESSFULLY
                    };
                }
                if (isUserRegistered.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) && isUserRegistered.Data is not null)
                {
                    transaction.Complete();
                    return new Return<LoginWithGoogleMoblieResDto>
                    {
                        Data = new LoginWithGoogleMoblieResDto
                        {
                            BearerToken = CreateBearerTokenAccount(isUserRegistered.Data.Id),
                            Name = isUserRegistered.Data.FullName,
                            Email = isUserRegistered.Data.Email,
                            Avatar = payload.Picture
                        },
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.LOGIN_SUCCESSFULLY
                    };
                }
                else
                {
                    transaction.Dispose();
                    return new Return<LoginWithGoogleMoblieResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = isUserRegistered.InternalErrorMessage };
                }
            }
            catch (Exception ex)
            {
                return new Return<LoginWithGoogleMoblieResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        #region Private methods
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
                Expires = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).AddDays(7),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return token == null ? throw new Exception(ErrorEnumApplication.SERVER_ERROR) : tokenHandler.WriteToken(token);
        }
        #endregion
    }
}
