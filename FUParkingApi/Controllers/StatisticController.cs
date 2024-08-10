using FUParkingApi.HelperClass;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [Route("api/statistic")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class StatisticController : Controller
    {
        private readonly ILogger<StatisticController> _logger;
        private readonly ISessionService _sessionService;
        private readonly IPaymentService _paymentService;

        public StatisticController(ILogger<StatisticController> logger, ISessionService sessionService, IPaymentService paymentService)
        {
            _logger = logger;
            _sessionService = sessionService;
            _paymentService = paymentService;
        }

        [HttpGet("session")]        
        public async Task<IActionResult> StatisticSessionAppAsync()
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _sessionService.StatisticSessionAppAsync();
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at GetSessionInOneMonthByParkingAreaAsync: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("customer/park")]
        public async Task<IActionResult> StatisticPaymentByCustomerAsync()
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _paymentService.StatisticPaymentByCustomerAsync();
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at GetSessionInOneMonthByParkingAreaAsync: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("customer/payment/method")]
        public async Task<IActionResult> StatisticSessionPaymentMethodByCustomerAsync()
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _paymentService.StatisticSessionPaymentMethodByCustomerAsync();
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at GetSessionInOneMonthByParkingAreaAsync: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }
    }
}
