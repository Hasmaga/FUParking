using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject.Common;
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
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _walletService.GetTransactionWalletMainAsync(req);
            if (!result.Message.Equals(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get transaction wallet main: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("transaction/extra")]
        public async Task<IActionResult> GetTransactionWalletExtra([FromQuery] GetListObjectWithFillerDateReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _walletService.GetTransactionWalletExtraAsync(req);
            if (!result.Message.Equals(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get transaction wallet extra: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("balance/main")]
        public async Task<IActionResult> GetBalanceWalletMain()
        {
            var result = await _walletService.GetBalanceWalletMainAsync();
            if (!result.Message.Equals(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get balance wallet main: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("balance/extra")]
        public async Task<IActionResult> GetBalanceWalletExtra()
        {
            var result = await _walletService.GetBalanceWalletExtraAsync();
            if (!result.Message.Equals(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get balance wallet extra: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("balance/{id}")]
        public async Task<IActionResult> GetBalanceWalletMainExtraAsync(Guid id)
        {
            var result = await _walletService.GetBalanceWalletMainExtraAsync(id);
            if (!result.Message.Equals(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get balance wallet: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }
    }
}
