using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/initialize-data")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class InitializeDataController : Controller
    {
        private readonly IInitializeDataService _initializeDataService;

        public InitializeDataController(IInitializeDataService initializeDataService)
        {
            _initializeDataService = initializeDataService;
        }

        [HttpGet("init-data")]
        public async Task<IActionResult> InitializeDatabase()
        {
            var result = await _initializeDataService.InitializeDatabase();
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
