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
    [Route("api/vehicles")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class VehicleController : Controller
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpPost("types")]
        public async Task<IActionResult> CreateVehicleType([FromBody] CreateVehicleTypeReqDto reqDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        maotou => maotou.Key,
                        maotou => maotou.Value?.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                    return StatusCode(422, new Return<Dictionary<string, List<string>?>>
                    {
                        Data = errors,
                        IsSuccess = false,
                        Message = ErrorEnumApplication.INVALID_INPUT
                    });
                }
                var result = await _vehicleService.CreateVehicleTypeAsync(reqDto);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            } catch (Exception)
            {
                return StatusCode(500, new Return<string>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        // PUT api/vehicles/types/{id}
        [HttpPut("types/{id}")]
        public async Task<IActionResult> UpdateVehicleType([FromRoute] Guid id, [FromBody] CreateVehicleTypeReqDto reqDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }

                UpdateVehicleTypeReqDto updateVehicleTypeReqDto = new UpdateVehicleTypeReqDto
                {
                    Id = id,
                    Name = reqDto.Name,
                    Description = reqDto.Description
                };

                var result = await _vehicleService.UpdateVehicleTypeAsync(updateVehicleTypeReqDto);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            } catch (Exception)
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
