using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    //[ApiController]
    [Route("api/areas")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class ParkingAreaController : Controller
    {
        private readonly IParkingAreaService _parkingAreaService;

        public ParkingAreaController(IParkingAreaService parkingAreaService)
        {
            _parkingAreaService = parkingAreaService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateParkingAreaAsync([FromBody] CreateParkingAreaReqDto req)
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
                var result = await _parkingAreaService.CreateParkingAreaAsync(req);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new Return<string>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,

                    InternalErrorMessage = e,
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetParkingAreasAsync(int pageIndex, int pageSize)
        {
            try
            {
                var result = await _parkingAreaService.GetParkingAreasAsync(pageIndex, pageSize);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateParkingAreaAsync([FromRoute] Guid id, [FromBody] CreateParkingAreaReqDto req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }

                UpdateParkingAreaReqDto updateParkingAreaReqDto = new()
                {
                    ParkingAreaId = id,
                    Name = req.Name,
                    Description = req.Description,
                    Block = req.Block,
                    MaxCapacity = req.MaxCapacity,
                    Mode = req.Mode,
                };

                var result = await _parkingAreaService.UpdateParkingAreaAsync(updateParkingAreaReqDto);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new Return<string>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,

                    InternalErrorMessage = e,
                });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParkingArea([FromRoute] Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(500, Helper.GetValidationErrors(ModelState));
                }

                var result = await _parkingAreaService.DeleteParkingArea(id);
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
    }
}
