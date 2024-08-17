using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Gate;
using FUParkingModel.ReturnCommon;
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
        private readonly ILogger<GateController> _logger;

        public GateController(IGateService gateService, ILogger<GateController> logger)
        {
            _gateService = gateService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("/api/gates")]
        public async Task<IActionResult> GetListGate(GetListObjectWithFiller req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _gateService.GetAllGateAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when get list gate: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGateAsync([FromBody] CreateGateReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _gateService.CreateGateAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when create gate: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGateAsync([FromRoute] Guid id, [FromBody] UpdateGateReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _gateService.UpdateGateAsync(req, id);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when update gate: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }


        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGate([FromRoute] Guid id)
        {
            var result = await _gateService.DeleteGate(id);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when delete gate: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [Authorize]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatusGateAsync([FromBody] UpdateStatusGateReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _gateService.UpdateStatusGateAsync(req.GateId, req.IsActive);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when update status gate: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }
    }
}
