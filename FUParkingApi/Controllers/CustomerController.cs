using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Customer;
using FUParkingModel.ResponseObject.Customer;
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
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ICustomerService customerService, IVehicleService vehicleService, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _vehicleService = vehicleService;
            _logger = logger;
        }

        [HttpPost("free")]
        public async Task<IActionResult> CreateNonPaidCustomerAsync([FromBody] CustomerReqDto req)
        {
            Return<dynamic> res = new() { Message = ErrorEnumApplication.SERVER_ERROR };
            try
            {
                if (!ModelState.IsValid)
                {
                    return UnprocessableEntity(Helper.GetValidationErrors(ModelState));
                }               

                res = await _customerService.CreateCustomerAsync(req);
                if (res.Message.Equals(ErrorEnumApplication.NOT_AUTHORITY))
                {
                    return Unauthorized(res);
                }

                if (res.Message.Equals(ErrorEnumApplication.EMAIL_IS_EXIST))
                {
                    return Conflict(res);
                }

                if (res.Message.Equals(ErrorEnumApplication.BANNED))
                {
                    return Forbid();
                }

                if (!res.IsSuccess)
                {
                    return BadRequest(res);
                }
                return Ok(res);
            }
            catch
            {
                return StatusCode(502, res);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerListAsync(GetCustomersWithFillerReqDto req)
        {                        
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }
                var res = await _customerService.GetListCustomerAsync(req);
                if (!res.IsSuccess)
                {
                    switch (res.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(401, res);
                        case ErrorEnumApplication.BANNED:
                            return StatusCode(403, res);
                        default:
                            if (res.InternalErrorMessage is not null)
                            {
                                _logger.LogError("Error at get list customer: {ex}", res.InternalErrorMessage);
                            }
                            return StatusCode(500, res);
                    }
                }
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error at get list customer: {ex}", ex);
                return StatusCode(500, new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR });
            }
        }        

        [Authorize]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateCustomerStatusAsync([FromBody] ChangeStatusCustomerReqDto req)
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

                var result = await _customerService.ChangeStatusCustomerAsync(req);
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
