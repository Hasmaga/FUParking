using FUParkingApi.HelperClass;
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
    [Route("api/gates")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class GateController : Controller
    {
        private readonly IGateService _gateService;

        public GateController(IGateService gateService)
        {
            _gateService = gateService;
        }

        [HttpGet]
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGateAsync([FromRoute] Guid id, [FromBody] UpdateGateReqDto req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }

                var result = await _gateService.UpdateGateAsync(req, id);
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
