using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Customer;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [Route("api/customers")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [HttpPost("free")]
        public async Task<IActionResult> CreateNonPaidCustomerAsync([FromBody] CustomerReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _customerService.CreateCustomerAsync(req);
            if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when create non-paid customer: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerListAsync(GetCustomersWithFillerReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var res = await _customerService.GetListCustomerAsync(req);
            if (!res.IsSuccess)
            {
                if (res.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when get list customer: {ex}", res.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(res.Message);
            }
            return Ok(res);
        }

        [Authorize]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateCustomerStatusAsync([FromBody] ChangeStatusCustomerReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _customerService.ChangeStatusCustomerAsync(req);
            if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when update customer status: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateCustomerAsync([FromBody] UpdateCustomerAccountReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _customerService.UpdateCustomerAccountAsync(req);
            if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when update customer: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpPost("nonpaid")]
        [Authorize]
        public async Task<IActionResult> CreateNonPaidCustomerAsync([FromBody] CreateNonPaidCustomerReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _customerService.CreateNonPaidCustomerAsync(req);
            if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when create non-paid customer: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCustomerAsync([FromRoute] Guid id)
        {            
            var result = await _customerService.DeleteCustomerByStaff(id);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when delete customer: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpPut("customer")]
        [Authorize]
        public async Task<IActionResult> UpdateCustomerByStaffAsync([FromBody] UpdateInformationCustomerResDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _customerService.UpdateInformationCustomerByStaff(req);
            if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when update customer: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }
    }
}
