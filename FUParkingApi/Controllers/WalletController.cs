using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly ILogger<WalletController> _logger;

        public WalletController(IWalletService walletService, ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _logger = logger;
        }

        [HttpGet("transaction")]
        public async Task<IActionResult> GetWalletTransaction([FromQuery] int pageSize = Pagination.PAGE_SIZE, [FromQuery] int pageIndex = Pagination.PAGE_INDEX, [FromQuery] int numberOfDays = 7)
        {
            Return<List<Transaction>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                string? userIdToken = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
                res = await _walletService.GetWalletTransactionByCustomerIdAsync(userIdToken, pageIndex, pageSize, numberOfDays);
                if (res.Data == null)
                {
                    return NotFound(res);
                }
                return Ok(res);
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
                return StatusCode(502, res);
            }
        }

        [HttpGet("transaction/main")]
        public async Task<IActionResult> GetTransactionWalletMain([FromQuery] GetListObjectWithFillerDateReqDto req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }
                var result = await _walletService.GetTransactionWalletMainAsync(req);
                if (!result.Message.Equals(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY))
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(409, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY });
                        default:
                            _logger.LogError("Error when get transaction wallet main: " + result.InternalErrorMessage);
                            return StatusCode(500, new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR });
                    }
                }
                return StatusCode(200, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error when get transaction wallet main: {ex}", ex.Message);
                return StatusCode(500, new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpGet("transaction/extra")]
        public async Task<IActionResult> GetTransactionWalletExtra([FromQuery] GetListObjectWithFillerDateReqDto req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }
                var result = await _walletService.GetTransactionWalletExtraAsync(req);
                if (!result.Message.Equals(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY))
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(409, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY });
                        default:
                            _logger.LogError("Error when get transaction wallet extra: " + result.InternalErrorMessage);
                            return StatusCode(500, new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR });
                    }
                }
                return StatusCode(200, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error when get transaction wallet extra: {ex}", ex.Message);
                return StatusCode(500, new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }
    }
}
