using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ReturnCommon;
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

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;            
        }

        [HttpGet("deposit")]
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
                            return StatusCode(403, new { message = "You do not have permission to access this resource." });
                        case ErrorEnumApplication.SERVER_ERROR:
                            return StatusCode(500, new { message = "Internal server error." });
                        default:
                            return StatusCode(500, new { message = "Internal server error." });
                    }
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
    }
}
