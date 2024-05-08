using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FUParkingApi.Controllers
{
    [Route("authentication_api")]
    public class AuthenticationController : ControllerBase
    {
        public AuthenticationController()
        {
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var props = new AuthenticationProperties { RedirectUri = "authentication_api/signin-google" };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }


        [HttpGet("signin-google")]
        public async Task<IActionResult> SignInGoogle()
        {
            var reponse = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (reponse.Principal == null)
            {
                return BadRequest("Error");
            }
            var name = reponse.Principal.FindFirst(ClaimTypes.Name).Value;
            var email = reponse.Principal.FindFirst(ClaimTypes.Email).Value;
            var givenName = reponse.Principal.FindFirst(ClaimTypes.GivenName).Value;

            return Ok(new { name, email, givenName });
        }
    }
}
