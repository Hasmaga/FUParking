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
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _vehicleService.GetVehicleTypesAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get Vehicle type");
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("type")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleTypesByCustomerAsync()
        {
            var result = await _vehicleService.GetListVehicleTypeByCustomer();
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get vehicle type by customer: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPost("types")]
        public async Task<IActionResult> CreateVehicleType([FromBody] CreateVehicleTypeReqDto reqDto)
        {

            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _vehicleService.CreateVehicleTypeAsync(reqDto);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get Vehicle type");
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        // PUT api/vehicles/types/{id}
        [HttpPut("types/{id}")]
        public async Task<IActionResult> UpdateVehicleType([FromRoute] Guid id, [FromBody] UpdateVehicleTypeReqDto reqDto)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _vehicleService.UpdateVehicleTypeAsync(id, reqDto);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get Vehicle type");
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetVehiclesAsync(GetListObjectWithFillerAttributeAndDateReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _vehicleService.GetVehiclesAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get Vehicle type");
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        // DELETE api/vehicles/types/{id}
        [HttpDelete("types/{id}")]
        public async Task<IActionResult> DeleteVehicleType([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _vehicleService.DeleteVehicleTypeAsync(id);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get Vehicle type");
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }


        [HttpPost("customer")]
        public async Task<IActionResult> CreateCustomerVehicle(CreateCustomerVehicleReqDto reqDto)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _vehicleService.CreateCustomerVehicleAsync(reqDto);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get Vehicle type");
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPut("customer")]
        public async Task<IActionResult> UpdateCustomerVehicle([FromBody] UpdateCustomerVehicleReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _vehicleService.UpdateVehicleInformationAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at update customer vehicle: {ex}", result.InternalErrorMessage);
                }                
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpGet("customer")]
        [Authorize]
        public async Task<IActionResult> GetCustomerVehicleByCustomerIdAsync()
        {

            var result = await _vehicleService.GetCustomerVehicleByCustomerIdAsync();
            if (!result.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at GetCustomerVehicleByCustomerIdAsync: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);

        }

        [HttpDelete("customer/{id}")]
        public async Task<IActionResult> DeleteCustomerVehicle([FromRoute] Guid id)
        {

            var result = await _vehicleService.DeleteVehicleByCustomerAsync(id);
            if (!result.Message.Equals(SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at delete customer vehicle: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);

        }
    }
}
