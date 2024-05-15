using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingService;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaffAsync(CreateUserReqDto staff)
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
                var result = await _userService.CreateStaffAsync(staff);
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
