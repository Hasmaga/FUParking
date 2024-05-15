using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingModel.ResponseObject;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Reactive.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Authorize(AuthenticationSchemes = "Google")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("login-google")]
        public IActionResult Login()
        {
            var props = new AuthenticationProperties { RedirectUri = "auth/signin-google" };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("signin-google")]
        public async Task<IActionResult> SignInGoogle()
        {
            try
            {
                var reponse = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
                if (reponse.Principal == null)
                {
                    return BadRequest("Error");
                }
                GoogleReturnAuthenticationResDto login = new()
                {
                    Name = (reponse.Principal.FindFirst(ClaimTypes.Name) ?? new Claim(ClaimTypes.Name, "No Name")).Value,
                    Email = (reponse.Principal.FindFirst(ClaimTypes.Email) ?? new Claim(ClaimTypes.Email, "No Email")).Value,
                    GivenName = (reponse.Principal.FindFirst(ClaimTypes.GivenName) ?? new Claim(ClaimTypes.GivenName, "No GivenName")).Value,
                    IsAuthentication = (reponse.Principal.Identity ?? new ClaimsIdentity()).IsAuthenticated
                };
                var result = await _authService.LoginWithGoogleAsync(login);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginWithCredentialReqDto login)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList()
                );
                return StatusCode(422, new Return<Dictionary<string, List<string>?>>
                {
                    Data = errors,
                    IsSuccess = false,
                    Message = ErrorEnumApplication.INVALID_INPUT
                });
            }

            var result = await _authService.LoginWithCredentialAsync(login);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }        
    }
}
