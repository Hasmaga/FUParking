using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using FUParkingService.VnPayService;
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
        private readonly IVnPayService _vnpayService;
        private readonly ILogger<DepositController> _logger;

        public DepositController(IZaloService zaloService, IVnPayService vnpayService, ILogger<DepositController> logger)
        {
            _zaloService = zaloService;
            _vnpayService = vnpayService;
            _logger = logger;
        }

        [HttpPost("zalopay/{packageId}")]
        public async Task<IActionResult> CustomerBuyPackageAsync([FromRoute] Guid packageId)
        {
            var result = await _zaloService.CustomerCreateRequestBuyPackageByZaloPayAsync(packageId);
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

        [HttpPost("vnpay/{packageId}")]
        public async Task<IActionResult> CustomerBuyPackageViaVnPayAsync([FromRoute] Guid packageId)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            if (ipAddress == null)
            {
                _logger.LogError("IP address is null.");
                return BadRequest("IP address is required.");
            }

            var result = await _vnpayService.CustomerCreateRequestBuyPackageByVnPayAsync(packageId, ipAddress);
            if (!result.Message.Equals(SuccessfullyEnumServer.SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when create request buy package by VnPay: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }
    }
}
