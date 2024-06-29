using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
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
                var result = await _gateService.GetAllGateAsync();
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

                    InternalErrorMessage = e,
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

                    InternalErrorMessage = e,
                });
            }
        }


        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGate([FromRoute] Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(500, Helper.GetValidationErrors(ModelState));
                }

                var result = await _gateService.DeleteGate(id);
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
