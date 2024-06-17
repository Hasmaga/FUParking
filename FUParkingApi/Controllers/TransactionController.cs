using FUParkingModel.Enum;
using FUParkingModel.Object;
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
        private readonly IHelpperService _helpperService;

        public TransactionController(ITransactionService transactionService, IHelpperService helpperService)
        {
            _transactionService = transactionService;
            _helpperService = helpperService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactionListAsync([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] int pageIndex = Pagination.PAGE_INDEX, [FromQuery] int pageSize = Pagination.PAGE_SIZE)
        {
            Return<List<Transaction>> res = new() { Message = ErrorEnumApplication.SERVER_ERROR };
            try
            {
                Guid userGuid = _helpperService.GetAccIdFromLogged();
                if (userGuid == Guid.Empty)
                {
                    return Unauthorized();
                }

                res = await _transactionService.GetTransactionsAsync(fromDate, toDate, pageSize, pageIndex, userGuid);

                if (res.Message.Equals(ErrorEnumApplication.NOT_AUTHORITY))
                {
                    return Unauthorized(res);
                }

                if (res.Message.Equals(ErrorEnumApplication.BANNED))
                {
                    return Forbid();
                }

                if (!res.IsSuccess)
                {
                    return BadRequest(res);
                }
                return Ok(res);
            }
            catch
            {
                return StatusCode(502, res);
            }
        }
    }
}
