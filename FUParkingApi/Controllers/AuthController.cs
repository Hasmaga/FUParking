using FUParkingApi.HelperClass;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FUParkingApi.Controllers
{
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
        [AllowAnonymous]
        public IActionResult Login()
        {
            var props = new AuthenticationProperties { RedirectUri = "api/auth/signin-google" };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("signin-google")]
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync([FromBody]LoginWithCredentialReqDto login)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
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

        [HttpGet("role")]
        [Authorize(AuthenticationSchemes = "Defaut")]
        public async Task<IActionResult> CheckRoleByTokenAsync()
        {
            var result = await _authService.CheckRoleByTokenAsync();
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
