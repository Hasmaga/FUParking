using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Vehicle;
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
        private readonly ILogger<VehicleController> _logger;

        public VehicleController(IVehicleService vehicleService, ILogger<VehicleController> logger)
        {
            _vehicleService = vehicleService;
            _logger = logger;
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetVehicleTypesAsync(GetListObjectWithFiller req)
        {
            try
            {
                var result = await _vehicleService.GetVehicleTypesAsync(req);
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

        [HttpGet("type")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleTypesByCustomerAsync()
        {
            try
            {
                var result = await _vehicleService.GetListVehicleTypeByCustomer();
                if (!result.Message.Equals(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY))
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.SERVER_ERROR:
                            _logger.LogError("Error at get vehicle types by customer: {ex}", result.Message);
                            return StatusCode(500, new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR });
                        default:
                            return BadRequest(result);
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at get vehicle types by customer: {ex}", ex.Message);
                return StatusCode(500, new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
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

        // PUT api/vehicles/types/{id}
        [HttpPut("types/{id}")]
        public async Task<IActionResult> UpdateVehicleType([FromRoute] Guid id, [FromBody] UpdateVehicleTypeReqDto reqDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }
                var result = await _vehicleService.UpdateVehicleTypeAsync(id, reqDto);
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

        [HttpGet]
        public async Task<IActionResult> GetVehiclesAsync()
        {
            try
            {
                var result = await _vehicleService.GetVehiclesAsync();
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

        // DELETE api/vehicles/types/{id}
        [HttpDelete("types/{id}")]
        public async Task<IActionResult> DeleteVehicleType([FromRoute] Guid id)
        {
            try
            {
                var result = await _vehicleService.DeleteVehicleTypeAsync(id);
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

        [HttpPost("customer")]
        public async Task<IActionResult> CreateCustomerVehicle(CreateCustomerVehicleReqDto reqDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }
                var result = await _vehicleService.CreateCustomerVehicleAsync(reqDto);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(409, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY });
                        case ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST:
                            return StatusCode(404, new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST });
                        case ErrorEnumApplication.PLATE_NUMBER_IS_EXIST:
                            return StatusCode(400, new Return<dynamic> { Message = ErrorEnumApplication.PLATE_NUMBER_IS_EXIST });
                        default:
                            _logger.LogError("Error at create customer vehicle: {ex}", result.InternalErrorMessage);
                            return BadRequest(result);
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at create customer vehicle: {ex}", ex.Message);
                return StatusCode(500, new Return<string>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpPut("customer")]
        public async Task<IActionResult> UpdateCustomerVehicle([FromBody] UpdateCustomerVehicleReqDto req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }
                var result = await _vehicleService.UpdateVehicleInformationAsync(req);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(409, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY });
                        case ErrorEnumApplication.VEHICLE_NOT_EXIST:
                            return StatusCode(404, new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_NOT_EXIST });
                        case ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST:
                            return StatusCode(404, new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST });
                        case ErrorEnumApplication.PLATE_NUMBER_IS_EXIST:
                            return StatusCode(400, new Return<dynamic> { Message = ErrorEnumApplication.PLATE_NUMBER_IS_EXIST });
                        default:
                            _logger.LogError("Error at update customer vehicle: {ex}", result.InternalErrorMessage);
                            return StatusCode(500, new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR });
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at update customer vehicle: {ex}", ex.Message);
                return StatusCode(500, new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpGet("customer")]
        [Authorize]
        public async Task<IActionResult> GetCustomerVehicleByCustomerIdAsync()
        {
            try
            {
                var result = await _vehicleService.GetCustomerVehicleByCustomerIdAsync();
                if (!result.Message.Equals(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY))
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(409, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY });
                        case ErrorEnumApplication.BANNED:
                            return StatusCode(409, new Return<dynamic> { Message = ErrorEnumApplication.BANNED });
                        default:
                            _logger.LogError("Error at get customer vehicle by customer: {ex}", result.Message);
                            return StatusCode(500, new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR });
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at get customer vehicle by customer: {ex}", ex.Message);
                return StatusCode(500, new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpDelete("customer/{id}")]
        public async Task<IActionResult> DeleteCustomerVehicle([FromRoute] Guid id)
        {
            try
            {
                var result = await _vehicleService.DeleteVehicleByCustomerAsync(id);
                if (!result.Message.Equals(SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY))
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(409, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY });
                        case ErrorEnumApplication.VEHICLE_NOT_EXIST:
                            return StatusCode(404, new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_NOT_EXIST });
                        default:
                            _logger.LogError("Error at delete customer vehicle: {ex}", result.InternalErrorMessage);
                            return StatusCode(500, new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR });
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at delete customer vehicle: {ex}", ex.Message);
                return StatusCode(500, new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }
    }
}
