using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.Object;
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
        private readonly IHelpperService _helperService;
        private readonly IVehicleService _vehicleService;

        public CustomerController(ICustomerService customerService, IHelpperService helpperService,IVehicleService vehicleService)
        {
            _customerService = customerService;
            _helperService = helpperService;
            _vehicleService = vehicleService;
        }
        
        [HttpPost("free")]
        public async Task<IActionResult> CreateNonPaidCustomerAsync([FromBody] CustomerReqDto req)
        {
            Return<Customer> res = new() { Message = ErrorEnumApplication.SERVER_ERROR };
            try
            {
                if(!ModelState.IsValid)
                {
                    return UnprocessableEntity(Helper.GetValidationErrors(ModelState));
                }
                Guid userGuid = _helperService.GetAccIdFromLogged();
                if(userGuid == Guid.Empty)
                {
                    return Unauthorized();
                }

                res = await _customerService.CreateCustomerAsync(req, userGuid);
                if(res.Message.Equals(ErrorEnumApplication.NOT_AUTHORITY))
                {
                    return Unauthorized(res);
                }

                if (res.Message.Equals(ErrorEnumApplication.EMAIL_IS_EXIST))
                {
                    return Conflict(res);
                }

                if(res.Message.Equals(ErrorEnumApplication.BANNED))
                {
                    return Forbid();
                }

                if(!res.IsSuccess)
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
            Return<List<GetCustomersWithFillerResDto>> res = new() { 
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                res = await _customerService.GetListCustomerAsync(req);
                if (res.Message.Equals(ErrorEnumApplication.NOT_AUTHORITY))
                {
                    return Unauthorized(res);
                }

                if(res.Message.Equals(ErrorEnumApplication.BANNED))
                {
                    return Forbid();
                }

                if(!res.IsSuccess)
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

        [HttpGet("vehicle")]
        public async Task<IActionResult> GetCustomerVehicleAsync()
        {
            Return<List<Vehicle>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                Guid customerGuid = _helperService.GetAccIdFromLogged();
                if(customerGuid == Guid.Empty)
                {
                    return Unauthorized();
                }

                res = await _vehicleService.GetCustomerVehicleByCustomerIdAsync(customerGuid);
                if (res.Message.ToLower().Equals(ErrorEnumApplication.BANNED))
                {
                    return Forbid();
                }

                if (res.Message.ToLower().Equals(ErrorEnumApplication.NOT_AUTHORITY))
                {
                    return Unauthorized(res);
                }
                if(!res.IsSuccess)
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

        [HttpGet("profile")]
        public async Task<IActionResult> GetCustomerProfile()
        {
            Return<Customer> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                Guid customerGuid = _helperService.GetAccIdFromLogged();
                if(customerGuid == Guid.Empty)
                {
                    return Unauthorized();
                }
                res = await _customerService.GetCustomerByIdAsync(customerGuid);

                if ((res.Message ?? "").ToLower().Equals(ErrorEnumApplication.BANNED))
                {
                    return Forbid();
                }
                if(!res.IsSuccess)
                {
                    return BadRequest(res);
                }
                return Ok(res);

            }catch
            {
                return StatusCode(502, res);
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
            } catch (Exception)
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
