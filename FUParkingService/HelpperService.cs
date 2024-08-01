using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FUParkingService
{
    public class HelpperService : IHelpperService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _http;
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;


        public HelpperService(IConfiguration configuration, IHttpContextAccessor http, IUserRepository userRepository, ICustomerRepository customerRepository)
        {
            _configuration = configuration;
            _http = http;
            _userRepository = userRepository;
            _customerRepository = customerRepository;
        }

        public bool CheckBearerTokenIsValidAndNotExpired(string token)
        {
            var securityKey = _configuration.GetSection("AppSettings:Token").Value ?? throw new Exception(ErrorEnumApplication.SERVER_ERROR);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                }, out SecurityToken validatedToken);
                // Check Token Is Expired
                if (validatedToken.ValidTo < DateTime.Now)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Guid GetAccIdFromLogged()
        {
            var AccId = _http.HttpContext?.User.FindFirst(ClaimTypes.Sid)?.Value;
            return AccId == null ? Guid.Empty : Guid.Parse(AccId);
        }

        public bool IsTokenValid()
        {
            var token = _http.HttpContext?.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            if (token == null || !CheckBearerTokenIsValidAndNotExpired(token))
            {
                return false;
            }
            return true;
        }

        public Task<string> CreatePassHashAndPassSaltAsync(string pass, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            return Task.FromResult(Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(pass))));
        }

        public async Task<Return<User>> ValidateUserAsync(string actor)
        {
            if (!IsTokenValid())
            {
                return new Return<User>
                {
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                };
            }
            var accountLogged = await _userRepository.GetUserByIdAsync(GetAccIdFromLogged());
            if (!accountLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || accountLogged.Data == null || accountLogged.Data.Role?.Name == null)
            {
                return new Return<User>
                {
                    InternalErrorMessage = accountLogged.InternalErrorMessage,
                    Message = ErrorEnumApplication.NOT_AUTHORITY
                };
            }
            if (accountLogged.Data.WrongPassword >= 5)
            {
                return new Return<User>
                {
                    Message = ErrorEnumApplication.ACCOUNT_IS_LOCK
                };
            }
            if (accountLogged.Data.StatusUser.Equals(StatusUserEnum.INACTIVE))
            {
                return new Return<User>
                {
                    Message = ErrorEnumApplication.ACCOUNT_IS_BANNED
                };
            }
            switch (actor)
            {
                case RoleEnum.STAFF:
                    if (!Auth.AuthStaff.Contains(accountLogged.Data.Role.Name))
                    {
                        return new Return<User>
                        {
                            Message = ErrorEnumApplication.NOT_AUTHORITY
                        };
                    }
                    break;
                case RoleEnum.SUPERVISOR:
                    if (!Auth.AuthSupervisor.Contains(accountLogged.Data.Role.Name))
                    {
                        return new Return<User>
                        {
                            Message = ErrorEnumApplication.NOT_AUTHORITY
                        };
                    }
                    break;
                case RoleEnum.MANAGER:
                    if (!Auth.AuthManager.Contains(accountLogged.Data.Role.Name))
                    {
                        return new Return<User>
                        {
                            Message = ErrorEnumApplication.NOT_AUTHORITY
                        };
                    }
                    break;
                default:
                    return new Return<User>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
            }
            return new Return<User>
            {
                Message = SuccessfullyEnumServer.SUCCESSFULLY,
                Data = accountLogged.Data,
                IsSuccess = true
            };
        }

        public async Task<Return<Customer>> ValidateCustomerAsync()
        {
            if (!IsTokenValid())
            {
                return new Return<Customer>
                {
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                };
            }
            var accountLogged = await _customerRepository.GetCustomerByIdAsync(GetAccIdFromLogged());
            if (!accountLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || accountLogged.Data == null)
            {
                return new Return<Customer>
                {
                    InternalErrorMessage = accountLogged.InternalErrorMessage,
                    Message = ErrorEnumApplication.NOT_AUTHORITY
                };
            }
            if (accountLogged.Data.StatusCustomer.Equals(StatusCustomerEnum.INACTIVE))
            {
                return new Return<Customer>
                {
                    Message = ErrorEnumApplication.ACCOUNT_IS_BANNED
                };
            }
            return new Return<Customer>
            {
                Message = SuccessfullyEnumServer.SUCCESSFULLY,
                Data = accountLogged.Data,
                IsSuccess = true
            };
        }
    }
}