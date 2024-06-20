using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
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
                if (isUserRegistered.Data != null)
                {
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
                            Message = SuccessfullyEnumServer.SUCCESSFULLY
                        };
                    }
                    return new Return<LoginResDto>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = isUserRegistered?.InternalErrorMessage
                    };
                }
                // Get the customer type
                var customerType = await _customerRepository.GetCustomerTypeByNameAsync(CustomerTypeEnum.PAID);
                if (customerType.Data == null)
                {
                    return new Return<LoginResDto> { Message = ErrorEnumApplication.GET_OBJECT_ERROR, InternalErrorMessage = customerType.InternalErrorMessage };
                }
                Customer newCustomer = new() { FullName = login.Name, Email = login.Email, CustomerTypeId = customerType.Data.Id, StatusCustomer = StatusCustomerEnum.ACTIVE };
                var result = await _customerRepository.CreateNewCustomerAsync(newCustomer);
                if (result.IsSuccess == false || result.Data == null)
                {
                    transaction.Dispose();
                    return new Return<LoginResDto> { Message = ErrorEnumApplication.ADD_OBJECT_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                }
                // Create waller for the new customer
                Wallet cusWalletMain = new() { WalletType = WalletType.MAIN, CustomerId = result.Data.Id, WalletStatus = StatusWalletEnum.ACTIVE };
                var resultWallet = await _walletRepository.CreateWalletAsync(cusWalletMain);
                if (resultWallet.IsSuccess == false || resultWallet.Data == null)
                {
                    transaction.Dispose();
                    return new Return<LoginResDto> { Message = ErrorEnumApplication.ADD_OBJECT_ERROR, InternalErrorMessage = resultWallet.InternalErrorMessage };
                }
                Wallet cusWalletExtra = new() { CustomerId = result.Data.Id, WalletType = WalletType.EXTRA, WalletStatus = StatusWalletEnum.ACTIVE };
                var resultWalletExtra = await _walletRepository.CreateWalletAsync(cusWalletExtra);
                if (resultWalletExtra.IsSuccess == false || resultWalletExtra.Data == null)
                {
                    transaction.Dispose();
                    return new Return<LoginResDto> { Message = ErrorEnumApplication.ADD_OBJECT_ERROR, InternalErrorMessage = resultWalletExtra.InternalErrorMessage };
                }
                transaction.Complete();

                return new Return<LoginResDto>
                {
                    Data = new LoginResDto
                    {
                        BearerToken = CreateBearerTokenAccount(result.Data.Id),
                        Name = result.Data.FullName,
                        Email = result.Data.Email
                    },
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                transaction.Dispose();
                return new Return<LoginResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex.Message };
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
                        Message = ErrorEnumApplication.CRENEDTIAL_IS_WRONG,
                        IsSuccess = false
                    };
                }
                // Check the password is correct
                if (!VerifyPasswordHash(req.Password, Convert.FromBase64String(isUserRegistered.Data.PasswordSalt ?? ""), isUserRegistered.Data.PasswordHash ?? ""))
                {
                    return new Return<LoginResDto>
                    {
                        Message = ErrorEnumApplication.CRENEDTIAL_IS_WRONG,
                        IsSuccess = false
                    };
                }
                return new Return<LoginResDto>
                {
                    Data = new LoginResDto
                    {
                        BearerToken = CreateBearerTokenAccount(isUserRegistered.Data.Id),
                        Name = isUserRegistered.Data.FullName,
                        Email = isUserRegistered.Data.Email,
                        Role = isUserRegistered.Data.Role?.Name
                    },
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<LoginResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<LoginResDto>> CheckRoleByTokenAsync()
        {
            try
            {
                // Check token is valid
                var isValid = _helpperService.IsTokenValid();
                if (isValid == false)
                {
                    return new Return<LoginResDto>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                var id = _helpperService.GetAccIdFromLogged();
                if (id == Guid.Empty)
                {
                    return new Return<LoginResDto>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user.IsSuccess == false || user.Data == null)
                {
                    return new Return<LoginResDto>
                    {
                        Message = ErrorEnumApplication.USER_NOT_EXIST,
                        IsSuccess = false
                    };
                }
                return new Return<LoginResDto>
                {
                    Data = new LoginResDto
                    {
                        Role = user.Data.Role?.Name
                    },
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<LoginResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<string>> LoginWithGoogleMobileAsync(string one_time_code)
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var clientSecrets = _configuration.GetSection("Authentication:Google:ClientSecret").Value;
                var clientId = _configuration.GetSection("Authentication:Google:ClientId").Value;
                if (string.IsNullOrEmpty(clientSecrets) || string.IsNullOrEmpty(clientId))
                {
                    return new Return<string> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var settings = new ValidationSettings() { Audience = [clientId] };
                Payload payload = await ValidateAsync(one_time_code, settings);
                if (payload == null)
                {
                    return new Return<string> { Message = ErrorEnumApplication.GOOGLE_LOGIN_FAILED };
                }
                if (!payload.HostedDomain.Equals("fpt.edu.vn"))
                {
                    return new Return<string> { Message = ErrorEnumApplication.NOT_EMAIL_FPT_UNIVERSITY };
                }
                var isUserRegistered = await _customerRepository.GetCustomerByEmailAsync(payload.Email);
                if (isUserRegistered.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    // Create new customer
                    var customerType = await _customerRepository.GetCustomerTypeByNameAsync(CustomerTypeEnum.PAID);
                    if (customerType.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT) || customerType.IsSuccess == false || customerType.Data == null)
                    {
                        return new Return<string> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customerType.InternalErrorMessage };
                    }
                    Customer newCustomer = new() {
                        FullName = payload.Name,
                        Email = payload.Email,
                        CustomerTypeId = customerType.Data.Id,
                        StatusCustomer = StatusCustomerEnum.ACTIVE,
                        Avarta = payload.Picture
                    };
                    var resultCreateCus = await _customerRepository.CreateNewCustomerAsync(newCustomer);
                    if (resultCreateCus.Message.Equals(ErrorEnumApplication.SERVER_ERROR) || resultCreateCus.Data == null)
                    {
                        transaction.Dispose();
                        return new Return<string> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = resultCreateCus.InternalErrorMessage };
                    }
                    // Create waller for the new customer
                    Wallet cusWalletMain = new() { 
                        WalletType = WalletType.MAIN, 
                        CustomerId = resultCreateCus.Data.Id, 
                        WalletStatus = StatusWalletEnum.ACTIVE 
                    };
                    var resultWallet = await _walletRepository.CreateWalletAsync(cusWalletMain);
                    if (resultWallet.Message.Equals(ErrorEnumApplication.SERVER_ERROR) || resultWallet.Data == null)
                    {
                        transaction.Dispose();
                        return new Return<string> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = resultWallet.InternalErrorMessage };
                    }
                    Wallet cusWalletExtra = new()
                    {
                        CustomerId = resultCreateCus.Data.Id, 
                        WalletType = WalletType.EXTRA, 
                        WalletStatus = StatusWalletEnum.ACTIVE 
                    };
                    var resultWalletExtra = await _walletRepository.CreateWalletAsync(cusWalletExtra);
                    if (resultWalletExtra.Message.Equals(ErrorEnumApplication.SERVER_ERROR) || resultWalletExtra.Data == null)
                    {
                        transaction.Dispose();
                        return new Return<string> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = resultWalletExtra.InternalErrorMessage };
                    }
                    transaction.Complete();
                    return new Return<string>
                    {
                        Data = CreateBearerTokenAccount(resultCreateCus.Data.Id),
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.SUCCESSFULLY
                    };
                } else if (isUserRegistered.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) && isUserRegistered.Data is not null)
                {
                    transaction.Complete();
                    return new Return<string>
                    {
                        Data = CreateBearerTokenAccount(isUserRegistered.Data.Id),
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.SUCCESSFULLY
                    };
                } else
                {
                    transaction.Dispose();
                    return new Return<string> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = isUserRegistered.InternalErrorMessage };
                }
            }
            catch (Exception ex)
            {
                return new Return<string>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,                    
                    InternalErrorMessage = ex.Message
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
