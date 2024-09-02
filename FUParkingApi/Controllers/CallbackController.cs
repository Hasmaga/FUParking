using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject.Zalo;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using FUParkingService.VnPayService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FUParkingApi.Controllers
{
    [Route("callback")]
    public class CallbackController : Controller
    {
        private readonly IZaloService _zaloService;
        private readonly IVnPayService _vnpayService;
        private readonly ILogger<CallbackController> _logger;

        public CallbackController(IZaloService zaloService, IVnPayService vnpayService, ILogger<CallbackController> logger)
        {
            _zaloService = zaloService;
            _vnpayService = vnpayService;
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
                if (!result.IsSuccess)
                {
                    if (result.InternalErrorMessage is not null)
                    {
                        _logger.LogInformation("Error at Callback Zalo: {result.InternalErrorMessage}", result.InternalErrorMessage);
                    }
                    return Helper.GetErrorResponse(result.Message);
                }
                return Ok(new Return<bool> { Message = SuccessfullyEnumServer.SUCCESSFULLY});
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at Callback Zalo: {ex}", ex);
                return StatusCode(500, new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CallbackZaloPayAsync([FromBody] dynamic cbdata)
        {            
            var result = new Dictionary<string, object>();            
            var dataStr = cbdata.GetProperty("data").GetString();
            var dataJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataStr);
            var appTransId = dataJson["app_trans_id"];
            Return<bool> result1 = await _zaloService.CallbackZaloPayAsync(appTransId);
            if (!result1.IsSuccess)
            {
                if (result1.InternalErrorMessage is not null)
                {
                    _logger.LogInformation("Error at Callback Zalo: {result1.InternalErrorMessage}", result1.InternalErrorMessage);
                }
                switch (result1.Message) {                    
                    default:
                        result["return_code"] = 0;
                        result["return_message"] = "error";
                        break;
                }
            }
            result["return_code"] = 1;
            result["return_message"] = "success";
            return Ok(result);
        }

        [HttpGet("vnpay")]
        public async Task<IActionResult> CallbackVnPayAsync()
        {
            var queryStringParameters = HttpContext.Request.Query;
            Return<bool> result = await _vnpayService.CallbackVnPayIPNUrl(queryStringParameters);
            if(!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogInformation("Error at Callback VnPay: {result.InternalErrorMessage}", result.InternalErrorMessage);
                }
            }

            return Ok(result);
        }
    }
}
