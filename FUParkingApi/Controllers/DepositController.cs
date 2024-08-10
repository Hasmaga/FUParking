using FUParkingApi.HelperClass;
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
        private readonly IZaloService _zaloService;
        private readonly ILogger<DepositController> _logger;

        public DepositController(IZaloService zaloService, ILogger<DepositController> logger)
        {
            _zaloService = zaloService;
            _logger = logger;
        }

        [HttpPost("{packetId}")]
        public async Task<IActionResult> CustomerBuyPackageAsync([FromRoute] Guid packetId)
        {
            var result = await _zaloService.CustomerCreateRequestBuyPackageByZaloPayAsync(packetId);
            if (!result.Message.Equals(SuccessfullyEnumServer.SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when create request buy package by ZaloPay: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }
    }
}
