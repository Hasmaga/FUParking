using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Session;
using FUParkingModel.ResponseObject.Session;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [Route("api/session")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class SessionController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly ILogger<SessionController> _logger;

        public SessionController(ISessionService sessionService, ILogger<SessionController> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckInAsync([FromForm] CreateSessionReqDto req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }
                var result = await _sessionService.CheckInAsync(req);
                if (!result.IsSuccess)
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(401, new Return<bool> { Message = ErrorEnumApplication.NOT_AUTHORITY });
                        case ErrorEnumApplication.CARD_NOT_EXIST:
                            return StatusCode(400, new Return<bool> { Message = ErrorEnumApplication.CARD_NOT_EXIST });
                        case ErrorEnumApplication.CARD_IN_USE:
                            return StatusCode(400, new Return<bool> { Message = ErrorEnumApplication.CARD_IN_USE });
                        case ErrorEnumApplication.PLATE_NUMBER_IN_USE:
                            return StatusCode(400, new Return<bool> { Message = ErrorEnumApplication.PLATE_NUMBER_IN_USE });
                        case ErrorEnumApplication.GATE_NOT_EXIST:
                            return StatusCode(400, new Return<bool> { Message = ErrorEnumApplication.GATE_NOT_EXIST });
                        case ErrorEnumApplication.PARKING_AREA_NOT_EXIST:
                            return StatusCode(400, new Return<bool> { Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST });
                        default:
                            _logger.LogError("Error at Check In: {ex}", result.InternalErrorMessage);
                            return StatusCode(500, new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR });
                    }
                }
                return StatusCode(200, new Return<bool> { IsSuccess = true, Message = SuccessfullyEnumServer.SUCCESSFULLY });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at Check In: {ex}", ex.Message);
                return StatusCode(500);
            }
        }

        [HttpPost("history")]
        public async Task<IActionResult> GetListSessionByCustomerAsync(GetListObjectWithFillerDateReqDto req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }
                var result = await _sessionService.GetListSessionByCustomerAsync(req);
                if (!result.IsSuccess)
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(401, new Return<bool> { Message = ErrorEnumApplication.NOT_AUTHORITY });                        
                        default:
                            _logger.LogError("Error at Get List Session By Customer: {ex}", result.InternalErrorMessage);
                            return StatusCode(500, new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR });
                    }
                }
                return StatusCode(200, new Return<IEnumerable<GetHistorySessionResDto>> { IsSuccess = true, Data = result.Data, Message = SuccessfullyEnumServer.SUCCESSFULLY });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at Get List Session By Customer: {ex}", ex.Message);
                return StatusCode(500);
            }
        }
    }
}
