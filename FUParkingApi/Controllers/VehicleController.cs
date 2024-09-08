using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Vehicle;
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
        [HttpPut("types")]
        public async Task<IActionResult> UpdateVehicleType([FromBody] UpdateVehicleTypeReqDto reqDto)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _vehicleService.UpdateVehicleTypeAsync(reqDto);
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

            var result = await _vehicleService.GetListCustomerVehicleByCustomerIdAsync();
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

        [HttpPut("types/{id}/status")]
        public async Task<IActionResult> ChangeStatusVehicleType([FromRoute] Guid id, [FromBody] bool isActive)
        {
            var result = await _vehicleService.ChangeStatusVehicleTypeAsync(id, isActive);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at change status vehicle type: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpPut("user/vehicle")]
        public async Task<IActionResult> ChangeStatusVehicleByUser([FromBody] UpdateNewCustomerVehicleByUseReqDto req)
        {
            var result = await _vehicleService.ChangeStatusVehicleByUserAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at change status vehicle by user: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpPut("user/vehicle/status")]
        public async Task<IActionResult> UpdateStatusInactiveAndActiveCustomerVehicleByUser([FromBody] UpdateStatusInactiveAndActiveCustomerVehicleByUserReqDto req)
        {
            var result = await _vehicleService.UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at update status inactive and active customer vehicle by user: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpGet("customer/{id}")]
        public async Task<IActionResult> GetCustomerVehicleByVehicleId([FromRoute] Guid id)
        {
            var result = await _vehicleService.GetCustomerVehicleByVehicleIdAsync(id);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get customer vehicle by vehicle id: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpPost("user/update")]
        public async Task<IActionResult> UpdateCustomerVehicleByUser([FromBody] UpdateCustomerVehicleByUserReqDto req)
        {
            var result = await _vehicleService.UpdateVehicleInformationByUserAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at update customer vehicle by user: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpGet("customers/{customerId}")]
        public async Task<IActionResult> GetVehicleByCustomerId([FromRoute] Guid customerId)
        {
            var result = await _vehicleService.GetListVehicleByCustomerIdForUserAsync(customerId);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get vehicle by customer id: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateListVehicleForCustomerByUser([FromBody] CreateListVehicleForCustomerByUserReqDto req)
        {
            var result = await _vehicleService.CreateListVehicleForCustomerByUserAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at create list vehicle for customer by user: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpDelete("{vehicleId}")]
        public async Task<IActionResult> DeleteVehicleByUserAsync([FromRoute] Guid vehicleId)
        {
            var result = await _vehicleService.DeleteVehicleByUserAsync(vehicleId);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at delete vehicle by user: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }
    }
}
