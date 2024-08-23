using FUParkingApi.HelperClass;
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
        private readonly ITransactionService _transactionService;
        private readonly ICustomerService _customerService;
        private readonly IVehicleService _vehicleService;
        private readonly ICardService _cardService;

        public StatisticController(ILogger<StatisticController> logger, ISessionService sessionService, IPaymentService paymentService, ITransactionService transactionService, IVehicleService vehicleService, ICustomerService customerService, ICardService cardService)
        {
            _logger = logger;
            _sessionService = sessionService;
            _paymentService = paymentService;
            _transactionService = transactionService;
            _vehicleService = vehicleService;
            _customerService = customerService;
            _cardService = cardService;
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

        [HttpGet("payment/method")]
        public async Task<IActionResult> StatisticSessionPaymentMethodAsync()
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _paymentService.StatisticSessionPaymentMethodAsync();
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

        [HttpGet("revenue/today")]
        public async Task<IActionResult> StatisticRevenueTodayAsync()
        {            
            var result = await _transactionService.GetRevenueTodayAsync();
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at GetRevenueTodayAsync: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("session/today")]
        public async Task<IActionResult> StatisticSessionTodayAsync()
        {            
            var result = await _sessionService.GetTotalSessionParkingTodayAsync();
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at GetTransactionTodayAsync: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("session/average")]
        public async Task<IActionResult> StatisticSessionAverageAsync()
        {            
            var result = await _sessionService.GetAverageSessionDurationPerDayAsync();
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at GetTransactionTodayAsync: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("customer")]
        public async Task<IActionResult> StatisticCustomerAsync()
        {            
            var result = await _customerService.StatisticCustomerAsync();
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

        [HttpGet("vehicle")]
        public async Task<IActionResult> StatisticVehicleAsync()
        {            
            var result = await _vehicleService.GetStatisticVehicleAsync();
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

        [HttpGet("session/checkin-checkout")]
        public async Task<IActionResult> StatisticCheckInCheckOutAsync()
        {            
            var result = await _sessionService.GetStatisticCheckInCheckOutAsync();
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

        [HttpGet("card")]
        public async Task<IActionResult> StatisticCardAsync()
        {            
            var result = await _cardService.GetStatisticCardAsync();
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

        [HttpGet("parkingarea/renvenue")]
        public async Task<IActionResult> StatisticParkingAreaRevenueAsync()
        {            
            var result = await _transactionService.GetListStatisticParkingAreaRevenueAsync();
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

        [HttpGet("parkingarea/{parkingId}/today")]
        public async Task<IActionResult> StatisticCheckInCheckOutInParkingAreaAsync([FromRoute] Guid parkingId)
        {
            var result = await _sessionService.GetStatisticCheckInCheckOutInParkingAreaAsync(parkingId);
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
