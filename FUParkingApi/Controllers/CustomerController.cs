using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/customer")]
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

        [HttpGet]
        public async Task<IActionResult> GetCustomerListAsync([FromQuery] int pageSize = Pagination.PAGE_SIZE, [FromQuery]int pageIndex = Pagination.PAGE_INDEX)
        {
            Return<List<Customer>> res = new() { 
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                Guid userId =  _helperService.GetAccIdFromLogged();
                if(userId == Guid.Empty)
                {
                    return Unauthorized();
                }

                res = await _customerService.GetListCustomerAsync(userId,pageSize, pageIndex);
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
        public async Task<IActionResult> UpdateCustomerStatusAsync(ChangeStatusCustomerReqDto req)
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
