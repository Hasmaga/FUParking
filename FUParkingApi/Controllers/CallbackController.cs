using FUParkingModel.Enum;
using FUParkingModel.RequestObject.Zalo;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [Route("callback")]
    public class CallbackController : Controller
    {
        private readonly IZaloService _zaloService;
        private readonly ILogger<CallbackController> _logger;

        public CallbackController(IZaloService zaloService, ILogger<CallbackController> logger)
        {
            _zaloService = zaloService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ZaloCallback req)
        {
            try
            {
                if (req.AppTransId == null || req.Status != 1)
                {
                    return StatusCode(400, new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR });
                }
                var result = await _zaloService.CallbackZaloPayAsync(req.AppTransId);
                if (result.IsSuccess)
                {
                    return StatusCode(200, result);
                }
                else
                {
                    return StatusCode(500, new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at Callback Zalo: {ex}", ex);
                return StatusCode(500, new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR });
            }
        }
    }
}
