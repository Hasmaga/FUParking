using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Session;
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
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _sessionService.CheckInAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at Check In: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOutAsync([FromForm] CheckOutAsyncReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _sessionService.CheckOutAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at Check Out: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetListSessionByCustomerAsync(GetListObjectWithFillerDateReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _sessionService.GetListSessionByCustomerAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at history: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPost("guest/checkin")]
        public async Task<IActionResult> GuestCheckInAsync(CheckInForGuestReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _sessionService.CheckInForGuestAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at Check In: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPost("payment")]
        public async Task<IActionResult> PaymentAsync(string CardNumber)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _sessionService.UpdatePaymentSessionAsync(CardNumber);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at payment: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPut("checkout/plateNumber")]
        public async Task<IActionResult> CheckOutSessionByPlateNumberAsync([FromForm] CheckOutSessionByPlateNumberReqDto req)
        {
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at Check Out: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("user/history")]
        public async Task<IActionResult> GetListSessionByUserAsync(GetListObjectWithFillerAttributeAndDateReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _sessionService.GetListSessionByUserAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at history: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("user/history/{id}")]
        public async Task<IActionResult> GetSessionBySessionIdAsync([FromRoute] Guid id)
        {
            var result = await _sessionService.GetSessionBySessionIdAsync(id);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at Get Session By Session Id: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPost("{SessionId}/cancel")]
        public async Task<IActionResult> CancelSessionBySessionIdAsync(Guid SessionId)
        {
            var result = await _sessionService.CancleSessionByIdAsync(SessionId);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at Cancel Session By Session Id: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("{parkingId}")]
        public async Task<IActionResult> GetListSessionStaffAsync([FromRoute] Guid parkingId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? cardNum, [FromQuery] string? plateNum, [FromQuery] int pageIndex=Pagination.PAGE_INDEX, [FromQuery] int pageSize= Pagination.PAGE_SIZE)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _sessionService.GetAllSessionByCardNumberAndPlateNumberAsync(parkingId, plateNum, cardNum, pageIndex, pageSize, startDate, endDate);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at today: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("card/{cardNumber}")]
        public async Task<IActionResult> GetNewestSessionByCardNumberAsync(string cardNumber)
        {
            var result = await _sessionService.GetNewestSessionByCardNumberAsync(cardNumber);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at Get Newest Session By Card Number: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("platenumber/{platenumber}")]
        public async Task<IActionResult> GetNewestSessionByPlateNumberAsync(string platenumber)
        {
            var result = await _sessionService.GetNewestSessionByPlateNumberAsync(platenumber);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at Get Newest Session By Plate Number: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPut("session/platenumber")]
        public async Task<IActionResult> UpdatePlateNumberInSessionAsync([FromForm] UpdatePlateNumberInSessionReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _sessionService.UpdatePlateNumberInSessionAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at Update Plate Number In Session: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }
    }
}
