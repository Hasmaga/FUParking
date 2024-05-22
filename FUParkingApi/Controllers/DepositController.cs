using FUParkingApi.HelperClass;
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
        public async Task<IActionResult> CustomerBuyPackageAsync([FromBody]BuyPackageReqDto request)
        {
            Return<bool> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                if(!ModelState.IsValid)
                {
                    return UnprocessableEntity(Helper.GetValidationErrors(ModelState));
                }
                string? userIdToken = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
                if(userIdToken == null)
                {
                    return Unauthorized();
                }
                Guid userId = new Guid(userIdToken);
                res = await _customerService.BuyPackageAsync(request, userId);
                if (!res.IsSuccess)
                {
                    return BadRequest(res);
                }
                return Ok(res);
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex.Message;
                return StatusCode(502, res);
            }
        }
    }
}
