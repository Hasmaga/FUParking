using FUParkingModel.Enum;
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

        public HelpperService(IConfiguration configuration, IHttpContextAccessor http)
        {
            _configuration = configuration;
            _http = http;
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
            var token = _http.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
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


    }
}