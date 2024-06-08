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

        [HttpGet("available")]
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
        
        [HttpGet("")]
        public async Task<IActionResult> GetCoinPackages()
        {
            try
            {
                var result = await _packageService.GetCoinPackages(null);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new Return<string>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveCoinPackages()
        {
            try
            {
                var result = await _packageService.GetCoinPackages(StatusPackageEnum.ACTIVE);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new Return<string>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
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
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }

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
        
        [HttpPut("{packageId}")]
        public async Task<IActionResult> UpdateCoinPackage([FromRoute] Guid packageId, [FromBody] UpdateCoinPackageReqDto updateCoinPackageReqDto)
        {
            Return<bool> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(422, Helper.GetValidationErrors(ModelState));
                }

                updateCoinPackageReqDto.PackageId = packageId;

                res = await _packageService.UpdateCoinPackage(updateCoinPackageReqDto);
                if (!res.IsSuccess)
                    return BadRequest(res);
                return Ok(res);
            }
            catch
            {
                return StatusCode(500, res);
            }
        }
        
        [HttpDelete("{packageId}")]
        public async Task<IActionResult> DeleteCoinPackage(Guid packageId)
        {
            Return<bool> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                res = await _packageService.DeleteCoinPackage(packageId);
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
