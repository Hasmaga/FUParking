using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingService;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/gate")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class GateController : Controller
    {
        private readonly IGateService _gateService;

        public GateController(IGateService gateService)
        {
            _gateService = gateService;
        }

        [Authorize]
        [HttpGet("/api/gates")]
        public async Task<IActionResult> GetListGate()
        {
            try
            {
                var result = await _gateService.GetAllGate();
                if (result.IsSuccess == false)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest(new Return<IEnumerable<Gate>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateGateAsync([FromBody] CreateGateReqDto req)
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
                var result = await _gateService.CreateGateAsync(req);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new Return<string>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,

                    InternalErrorMessage = e.Message,
                });
            }
        }
    }
}
