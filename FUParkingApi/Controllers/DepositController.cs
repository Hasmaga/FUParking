using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/deposit")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class DepositController : Controller
    {
        private readonly IDepositService _depositService;
        private readonly ICustomerService _customerService;
        public DepositController(IDepositService depositService,ICustomerService customerService)
        {
            _depositService = depositService;
            _customerService = customerService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CustomerBuyPackageAsync(BuyPackageReqDto request)
        {
            Return<object> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                if(!ModelState.IsValid)
                {
                    return UnprocessableEntity();
                }
                string? userIdToken = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
                if(userIdToken == null)
                {
                    return Unauthorized();
                }
                Guid userId = new Guid(userIdToken);
                res = await _customerService.BuyPackageAsync(request, userId);
                return Ok(res);
            }
            catch (Exception ex)
            {
                if(ex is EntryPointNotFoundException)
                {
                    res.Message = ex.Message;

                    return NotFound(res);
                }

                if(ex is OperationCanceledException)
                {
                    // Bad gateway server failed
                    res.Message = ex.Message;
                    return StatusCode(502, res);
                }
                return BadRequest(res);
            }
        }
    }
}
