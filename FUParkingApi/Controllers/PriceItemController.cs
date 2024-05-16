using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/price-item")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class PriceItemController : Controller
    {
        private readonly IPriceItemService _priceItemService;

        public PriceItemController(IPriceItemService priceItemService)
        {
            _priceItemService = priceItemService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePriceItemAsync(CreatePriceItemReqDto req)
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
                var result = await _priceItemService.CreatePriceItemAsync(req);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePriceItemAsync(Guid id)
        {
            try
            {                
                var result = await _priceItemService.DeletePriceItemAsync(id);
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

        [HttpGet("get-all-by-price-table-id/{id}")]
        public async Task<IActionResult> GetAllPriceItemAsync(Guid id)
        {
            try
            {
                var result = await _priceItemService.GetAllPriceItemByPriceTableAsync(id);
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
    }
}
