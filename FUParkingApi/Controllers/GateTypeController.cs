using FUParkingService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/gate-types")]
    public class GateTypeController : Controller
    {
        private readonly IGateTypeService _gateTypeService;

        public GateTypeController(IGateTypeService gateTypeService)
        {
            _gateTypeService = gateTypeService;
        }
    }
}
