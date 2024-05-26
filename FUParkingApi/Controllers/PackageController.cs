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
    [Route("api/packages")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class PackageController : Controller
    {
        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailablePackageAsync()
        {
            Return<List<Package>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                res = await _packageService.GetAvailablePackageAsync();
                if (!res.IsSuccess)
                    return BadRequest(res);
                return Ok(res);
            }
            catch 
            {
                return StatusCode(502, res);
            }
        }

        [HttpPut("{packageId}")]
        public async Task<IActionResult> UpdateCoinPackage([FromRoute] Guid packageId, [FromBody] UpdateCoinPackageReqDto updateCoinPackageReqDto)
        {
            Return<bool> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                res = await _packageService.UpdateCoinPackage(packageId, updateCoinPackageReqDto);
                if (!res.IsSuccess)
                    return BadRequest(res);
                return Ok(res);
            }
            catch
            {
                return StatusCode(500, res);
            }
        }
    }
}
