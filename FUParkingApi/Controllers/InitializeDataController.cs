using FUParkingService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [Route("initialize")]
    public class InitializeDataController : Controller
    {
        private readonly IInitializeDataService _initializeDataService;

        public InitializeDataController(IInitializeDataService initializeDataService)
        {
            _initializeDataService = initializeDataService;
        }

        [HttpGet("InitData")]
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
