using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject.Common;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/transaction")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetListTransactionPaymentAsync([FromQuery] GetListObjectWithFillerAttributeAndDateReqDto req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }
                var result = await _transactionService.GetListTransactionPaymentAsync(req);
                if (!result.IsSuccess)
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(403, new { message = ErrorEnumApplication.NOT_AUTHORITY });
                        default:
                            _logger.LogInformation("Error when get list transaction payment: {ex}", result.InternalErrorMessage);
                            return StatusCode(500, new { message = ErrorEnumApplication.SERVER_ERROR });
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error when get list transaction payment: {ex}", ex.Message);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
    }
}
