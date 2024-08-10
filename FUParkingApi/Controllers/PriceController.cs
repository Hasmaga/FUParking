using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Price;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
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
        public async Task<IActionResult> GetAllPriceItemByPriceTableAsync(Guid id)
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
        public async Task<IActionResult> CreatePriceTableAsync(CreatePriceTableReqDto req)
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
        public async Task<IActionResult> UpdatePriceTableStatusAsync(ChangeStatusPriceTableReqDto req)
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
        public async Task<IActionResult> GetAllPriceTableAsync()
        {
            var result = await _priceService.GetAllPriceTableAsync();
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
        public async Task<IActionResult> CreateDefaultPriceTableAsync(CreateDefaultPriceTableReqDto req)
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
        public async Task<IActionResult> CreateDefaultPriceItemForDefaultPriceTableAsync(CreateDefaultItemPriceReqDto req)
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
        public async Task<IActionResult> CreatePriceItemAsync(CreateListPriceItemReqDto req)
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
