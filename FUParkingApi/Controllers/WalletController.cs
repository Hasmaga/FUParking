using FUParkingModel.Enum;
using FUParkingModel.Object;
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

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("transaction")]
        public async Task<IActionResult> GetWalletTransaction([FromQuery] int pageSize = 5, [FromQuery] int pageIndex = 1, [FromQuery]int numberOfDays = 7)
        {
            Return<List<Transaction>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                string? userIdToken = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
                res = await _walletService.GetWalletTransactionByCustomerIdAsync(userIdToken, pageIndex, pageSize, numberOfDays);
                if(res.Data == null) {
                    return NotFound(res);
                }
                return Ok(res);
            }
            catch(Exception ex)
            {
                res.InternalErrorMessage = ex.Message;
                return StatusCode(502,res);
            }
        }

    }
}
