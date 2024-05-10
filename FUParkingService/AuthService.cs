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

        public Task<Return<CreateUserReqDto>> CreateUserAsync(CreateUserReqDto user)
        {
            throw new NotImplementedException();
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
                    if (result.IsSuccess == false)
                    {
                        transaction.Dispose();
                        return new Return<LoginResDto>
                        {
                            ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = result.InternalErrorMessage
                        };
                    }
                    transaction.Complete();
                    if (result.IsSuccess == true && result.Data != null)
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
