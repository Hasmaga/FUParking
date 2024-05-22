﻿using FUParkingModel.Enum;
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

        public CustomerController(ICustomerService customerService, IHelpperService helpperService)
        {
            _customerService = customerService;
            _helperService = helpperService;
        }

        [HttpGet]
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

                if (res.Message.ToLower().Equals(ErrorEnumApplication.BANNED))
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
