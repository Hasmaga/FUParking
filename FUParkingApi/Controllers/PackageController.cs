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

        [HttpPost]
        public async Task<IActionResult> CreateCoinPackage(CreateCoinPackageReqDto reqDto)
        {
            Return<bool> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                res = await _packageService.CreateCoinPackage(reqDto);
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
