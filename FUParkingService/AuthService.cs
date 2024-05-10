using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingModel.ReturnObject;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Transactions;

namespace FUParkingService
{
    public class AuthService : IAuthService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerTypeRepository _customerTypeRepository;
        private readonly IConfiguration _configuration;

        public AuthService(ICustomerRepository customerRepository, ICustomerTypeRepository customerTypeRepository, IConfiguration configuration)
        {
            _customerRepository = customerRepository;
            _customerTypeRepository = customerTypeRepository;
            _configuration = configuration;
        }

        public async Task<Return<string>> LoginWithGoogleAsync(GoogleReturnAuthenticationResDto login)
        {
            try
            {
                if (login.IsAuthentication == false)
                {
                    return new Return<string>
                    {
                        ErrorMessage = ErrorEnumApplication.GOOGLE_LOGIN_FAILED,
                        IsSuccess = false,
                    };
                }
                // Check the email is @fpt.edu.vn only
                if (!login.Email.Contains("@fpt.edu.vn"))
                {
                    return new Return<string>
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
                    using var transaction = new TransactionScope();
                    // Get the customer type
                    var customerType = await _customerTypeRepository.GetCustomerTypeByNameAsync(CustomerTypeEnum.PAID);
                    if (customerType.Data == null)
                    {
                        return new Return<string>
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
                    transaction.Complete();
                    if (result.IsSuccess == false)
                    {
                        return new Return<string>
                        {
                            ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = result.InternalErrorMessage
                        };
                    }
                    if (result.IsSuccess == true && result.Data != null)
                    {
                        return new Return<string>
                        {
                            Data = CreateBearerTokenAccount(result.Data.Id),
                            IsSuccess = true,
                            SuccessfullyMessage = SuccessfullyEnumServer.SUCCESSFULLY
                        };
                    }
                }
                if (isUserRegistered != null && isUserRegistered.Data != null)
                {
                    return new Return<string>
                    {
                        Data = CreateBearerTokenAccount(isUserRegistered.Data.Id),
                        IsSuccess = true,
                        SuccessfullyMessage = SuccessfullyEnumServer.SUCCESSFULLY
                    };
                }
                return new Return<string>
                {
                    ErrorMessage = ErrorEnumApplication.GOOGLE_LOGIN_FAILED,
                    IsSuccess = false,
                };
            }
            catch (Exception ex)
            {
                return new Return<string>
                {
                    ErrorMessage = ErrorEnumApplication.GOOGLE_LOGIN_FAILED,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        #region Private methods
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
