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

        [HttpPost("/api/price/item")]
        public async Task<IActionResult> CreatePriceItemAsync(CreateListPriceItemReqDto req)
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
                var result = await _priceService.CreatePriceItemAsync(req);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new Return<string>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpDelete("/api/price/item/{id}")]
        public async Task<IActionResult> DeletePriceItemAsync(Guid id)
        {
            try
            {
                var result = await _priceService.DeletePriceItemAsync(id);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new Return<string>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpGet("/api/price/{id}/items")]
        public async Task<IActionResult> GetAllPriceItemAsync(Guid id)
        {
            try
            {
                var result = await _priceService.GetAllPriceItemByPriceTableAsync(id);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new Return<string>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpPost("/api/price")]
        public async Task<IActionResult> CreatePriceTableAsync(CreatePriceTableReqDto req)
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
                var result = await _priceService.CreatePriceTableAsync(req);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new Return<object>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpPut("/api/price/status")]
        public async Task<IActionResult> UpdatePriceTableStatusAsync(ChangeStatusPriceTableReqDto req)
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
                var result = await _priceService.UpdateStatusPriceTableAsync(req);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new Return<object>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpGet("/api/prices")]
        public async Task<IActionResult> GetAllPriceTableAsync()
        {
            try
            {
                var result = await _priceService.GetAllPriceTableAsync();
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new Return<object>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpPost("/api/price/vehicle/default")]
        public async Task<IActionResult> CreateDefaultPriceTableAsync(CreateDefaultPriceTableReqDto req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }
                var result = await _priceService.CreateDefaultPriceTableAsync(req);
                if (!result.IsSuccess)
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(403, result);
                        case ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST:
                            return StatusCode(404, result);
                        case ErrorEnumApplication.DEFAULT_PRICE_TABLE_IS_EXIST:
                            return StatusCode(400, result);
                        default:
                            _logger.LogError("Error at CreateDefaultPriceTableAsync: {ex}", result.InternalErrorMessage);
                            return StatusCode(500, result);
                    }
                }
                return StatusCode(200, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at CreateDefaultPriceTableAsync: {ex}", ex.Message);
                return StatusCode(500, new Return<object>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpPost("/api/price/vehicle/item/default")]
        public async Task<IActionResult> CreateDefaultPriceItemForDefaultPriceTableAsync(CreateDefaultItemPriceReqDto req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }
                var result = await _priceService.CreateDefaultPriceItemForDefaultPriceTableAsync(req);
                if (!result.IsSuccess)
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(403, result);
                        case ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST:
                            return StatusCode(404, result);
                        case ErrorEnumApplication.DEFAULT_PRICE_TABLE_IS_NOT_EXIST:
                            return StatusCode(404, result);
                        default:
                            _logger.LogError("Error at CreateDefaultPriceItemForDefaultPriceTableAsync: {ex}", result.InternalErrorMessage);
                            return StatusCode(500, result);
                    }
                }
                return StatusCode(200, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at CreateDefaultPriceItemForDefaultPriceTableAsync: {ex}", ex);
                return StatusCode(500, new Return<object>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpPost("/api/price/vehicle/item")]
        public async Task<IActionResult> CreateListPriceItemAsync(CreateListPriceItemReqDto req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }
                var result = await _priceService.CreatePriceItemAsync(req);
                if (!result.IsSuccess)
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(403, result);
                        case ErrorEnumApplication.PRICE_TABLE_NOT_EXIST:
                            return StatusCode(404, result);
                        default:
                            _logger.LogError("Error at CreateListPriceItemAsync: {ex}", result.Message);
                            return StatusCode(500, result);
                    }
                }
                return StatusCode(200, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at CreateListPriceItemAsync: {ex}", ex.Message);
                return StatusCode(500, new Return<object>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }
    }
}
