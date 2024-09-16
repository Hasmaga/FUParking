using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FUParkingApi.Controllers
{    
    [Route("api/user")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaffAsync([FromBody] CreateUserReqDto staff)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _userService.CreateStaffAsync(staff);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at create staff: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [Authorize]
        [HttpPost("supervisor")]
        public async Task<IActionResult> CreateSupervisorAsync([FromBody] CreateUserReqDto supervisor)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _userService.CreateSupervisorAsync(supervisor);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at create supervisor: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [Authorize]
        [HttpPost("manager")]
        public async Task<IActionResult> CreateManagerAsync([FromBody] CreateUserReqDto manager)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _userService.CreateManagerAsync(manager);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at create manager: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllUsersAsync(GetListObjectWithFiller req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _userService.GetListUserAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at get all users: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [Authorize]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatusUserAsync([FromQuery] Guid userId)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _userService.ChangeUserStatusAsync(userId);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at update status user: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [Authorize]
        [HttpPut("password-failures/reset")]
        public async Task<IActionResult> ResetWrongPasswordCountAsync([FromQuery] Guid userId)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _userService.ResetWrongPasswordCountAsync(userId);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at refresh wrong password count: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _userService.UpdateUserAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at update user: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [Authorize]
        [HttpPut("password")]
        public async Task<IActionResult> UpdatePasswordAsync([FromBody] UpdateUserPasswordReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _userService.UpdatePasswordAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at update password: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserByIdAsync(Guid id)
        {
            var result = await _userService.DeleteUserByIdAsync(id);
            if (!result.Message.Equals(SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at delete user by id: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [Authorize]
        [HttpPost("list")]
        public async Task<IActionResult> CreateListUserAsync([FromBody] FUParkingModel.RequestObject.User.CreateListUserReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _userService.CreateListUserAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error at create list user: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }
    }
}
