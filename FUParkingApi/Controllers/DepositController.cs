using FUParkingModel.Enum;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("{packetId}")]
        public async Task<IActionResult> CustomerBuyPackageAsync([FromRoute] Guid packetId)
        {
            try
            {
                var result = await _zaloService.CustomerCreateRequestBuyPackageByZaloPayAsync(packetId);
                if (result.IsSuccess)
                {
                    return StatusCode(200, result);
                }
                else
                {
                    return result.Message switch
                    {
                        ErrorEnumApplication.NOT_AUTHORITY => StatusCode(403, new Return<bool> { Message = ErrorEnumApplication.NOT_AUTHORITY }),
                        ErrorEnumApplication.PACKAGE_NOT_EXIST => StatusCode(404, new Return<bool> { Message = ErrorEnumApplication.PACKAGE_NOT_EXIST }),
                        _ => StatusCode(500, new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR }),
                    };
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
