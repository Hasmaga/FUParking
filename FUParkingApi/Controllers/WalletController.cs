using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                            _logger.LogError("Error when get transaction wallet main: {ex}", result.InternalErrorMessage);
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
                            _logger.LogError("Error when get transaction wallet extra: {ex}", result.InternalErrorMessage);
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

        [HttpGet("balance/main")]
        public async Task<IActionResult> GetBalanceWalletMain()
        {
            try
            {
                var result = await _walletService.GetBalanceWalletMainAsync();
                if (!result.Message.Equals(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY))
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(409, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY });
                        default:
                            _logger.LogError("Error when get balance wallet main: {ex}", result.InternalErrorMessage);
                            return StatusCode(500, new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR });
                    }
                }
                return StatusCode(200, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error when get balance wallet main: {ex}", ex.Message);
                return StatusCode(500, new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpGet("balance/extra")]
        public async Task<IActionResult> GetBalanceWalletExtra()
        {
            try
            {
                var result = await _walletService.GetBalanceWalletExtraAsync();
                if (!result.Message.Equals(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY))
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(409, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY });
                        default:
                            _logger.LogError("Error when get balance wallet extra: {ex}", result.InternalErrorMessage);
                            return StatusCode(500, new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR });
                    }
                }
                return StatusCode(200, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error when get balance wallet extra: {ex}", ex.Message);
                return StatusCode(500, new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }
    }
}
