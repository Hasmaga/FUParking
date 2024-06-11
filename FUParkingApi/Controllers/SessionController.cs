using FUParkingModel.Enum;
using FUParkingModel.RequestObject.Session;
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
                    var errors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList()
                        );
                    return StatusCode(422, new Return<Dictionary<string, List<string>?>>
                    {
                        Data = errors,
                        IsSuccess = false,
                        Message = ErrorEnumApplication.INVALID_INPUT
                    });
                }
                var result = await _sessionService.CheckInAsync(req);
                if (!result.IsSuccess)
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(401, new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY });
                        case ErrorEnumApplication.CARD_NOT_EXIST:
                            return StatusCode(400, new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.CARD_NOT_EXIST });
                        case ErrorEnumApplication.CARD_IN_USE:
                            return StatusCode(400, new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.CARD_IN_USE });
                        case ErrorEnumApplication.PLATE_NUMBER_IN_USE:
                            return StatusCode(400, new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.PLATE_NUMBER_IN_USE });
                        case ErrorEnumApplication.GATE_NOT_EXIST:
                            return StatusCode(400, new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.GATE_NOT_EXIST });
                        case ErrorEnumApplication.PARKING_AREA_NOT_EXIST:
                            return StatusCode(400, new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST });
                        default:
                            _logger.LogError("Error at Check In: " + result.InternalErrorMessage);
                            return StatusCode(500, new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.SERVER_ERROR });
                    }
                }
                return StatusCode(200, new Return<bool> { IsSuccess = true, Message = SuccessfullyEnumServer.SUCCESSFULLY });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at Check In: " + ex.Message);
                return StatusCode(500);
            }
        }
    }
}
