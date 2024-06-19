using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/deposit")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class DepositController : Controller
    {
        public readonly IZaloService _zaloService;
        
        public DepositController(IZaloService zaloService)
        {
            _zaloService = zaloService;
        }

        [HttpPost]
        public async Task<IActionResult> CustomerBuyPackageAsync([FromRoute]Guid packetId)
        {
            try
            {
                //var result = await _zaloService.CustomerCreateRequestBuyPackageByZaloPayAsync(packetId);
                //if (result.IsSuccess)
                //{
                //    return StatusCode(200, new Return<bool> { Message = SuccessfullyEnumServer.CREATE_DEPOSIT_SUCCESSFULLY });
                //}
                //else
                //{
                //    switch(result.Message)
                //    {
                //        case ErrorEnumApplication.NOT_AUTHORITY:
                //            return StatusCode(403, new Return<bool> { Message = ErrorEnumApplication.NOT_AUTHORITY });
                //        case ErrorEnumApplication.PACKAGE_NOT_EXIST:
                //            return StatusCode(404, new Return<bool> { Message = ErrorEnumApplication.PACKAGE_NOT_EXIST });
                //        case 
                //    }
                        
                //}
                return Ok();
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
