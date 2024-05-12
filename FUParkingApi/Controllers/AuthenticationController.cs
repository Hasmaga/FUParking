using FUParkingModel.RequestObject;
using FUParkingModel.ReturnObject;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FUParkingApi.Controllers
{
    [Route("auth")]
    public class AuthenticationController : Controller
    {
        private readonly IAuthService _authService;
        public AuthenticationController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("loginGoogle")]
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

        [HttpPost("loginWithCredential")]
        public async Task<IActionResult> LoginAsync(LoginWithCredentialReqDto login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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

        [Authorize]
        [HttpPost("CreateStaff")]
        public async Task<IActionResult> CreateStaffAsync(CreateUserReqDto staff)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authService.CreateStaffAsync(staff);
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
