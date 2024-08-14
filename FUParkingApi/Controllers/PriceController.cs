using FUParkingApi.HelperClass;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Price;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{    
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class PriceController : Controller
    {
        private readonly IPriceService _priceService;
        private readonly ILogger<PriceController> _logger;

        public PriceController(IPriceService priceService, ILogger<PriceController> logger)
        {
            _priceService = priceService;
            _logger = logger;
        }           

        [HttpGet("/api/price/{id}/items")]
        public async Task<IActionResult> GetAllPriceItemByPriceTableAsync([FromRoute] Guid id)
        {
            var result = await _priceService.GetAllPriceItemByPriceTableAsync(id);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get all price item by price table async: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPost("/api/price")]
        public async Task<IActionResult> CreatePriceTableAsync([FromBody] CreatePriceTableReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _priceService.CreatePriceTableAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at create price table async: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPut("/api/price/status")]
        public async Task<IActionResult> UpdatePriceTableStatusAsync([FromBody] ChangeStatusPriceTableReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _priceService.UpdateStatusPriceTableAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at update price table status async: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("/api/prices")]
        public async Task<IActionResult> GetAllPriceTableAsync([FromQuery] GetListObjectWithFillerAttributeAndDateReqDto req)
        {
            var result = await _priceService.GetAllPriceTableAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get all price table async: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPost("/api/price/vehicle/default")]
        public async Task<IActionResult> CreateDefaultPriceTableAsync([FromBody] CreateDefaultPriceTableReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _priceService.CreateDefaultPriceTableAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at create default price table async: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPost("/api/price/vehicle/item/default")]
        public async Task<IActionResult> CreateDefaultPriceItemForDefaultPriceTableAsync([FromBody] CreateDefaultItemPriceReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _priceService.CreateDefaultPriceItemForDefaultPriceTableAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at create default price item for default price table async: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPost("/api/price/vehicle/item")]
        public async Task<IActionResult> CreatePriceItemAsync([FromBody] CreateListPriceItemReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _priceService.CreatePriceItemAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at create price item async: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }
    }
}
