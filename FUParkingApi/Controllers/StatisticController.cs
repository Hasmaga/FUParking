using FUParkingApi.HelperClass;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;
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

        [HttpGet("session/parked")]
        public async Task<IActionResult> StatisticSessionParkedAsync()
        {
            var result = await _sessionService.GetTotalSessionParkedAsync();
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

        [HttpGet("payment/{gateId}/today")]
        public async Task<IActionResult> StatisticPaymentTodayForGateAsync([FromRoute] Guid gateId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var result = await _paymentService.GetStatisticPaymentTodayForGateAsync(gateId, startDate, endDate);
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

        [HttpGet("areas/revenue")]
        public async Task<IActionResult> GetListStatisticRevenueParkingAreasDetailsAsync([FromQuery] GetListObjectWithFillerDateAndSearchInputResDto req)
        {
            var result = await _transactionService.GetListStatisticRevenueParkingAreasDetailsAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at GetListStatisticRevenueParkingAreasDetailsAsync: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("parkings/revenue")]
        public async Task<IActionResult> GetListStatisticRevenueOfParkingSystemAsync([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var result = await _transactionService.GetListStatisticRevenueOfParkingSystemAsync(startDate, endDate);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at GetListStatisticRevenueOfParkingSystemAsync: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("parkings/{parkingId}/revenue")]
        public async Task<IActionResult> GetListStatisticRevenueOfParkingSystemDetailsAsync([FromRoute] Guid parkingId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var result = await _transactionService.GetListStatisticRevenueOfParkingSystemDetailsAsync(parkingId, startDate, endDate);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at GetListStatisticRevenueOfParkingSystemDetailsAsync: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }
    }
}
