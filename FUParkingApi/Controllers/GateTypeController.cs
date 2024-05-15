using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/gate-types")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class GateTypeController : Controller
    {
        private readonly IGateTypeService _gateTypeService;

        public GateTypeController(IGateTypeService gateTypeService)
        {
            _gateTypeService = gateTypeService;
        }
    }
}
