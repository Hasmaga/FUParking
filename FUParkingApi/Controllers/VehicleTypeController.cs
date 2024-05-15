using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/vehicle-types")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class VehicleTypeController : Controller
    {
        private readonly IVehicleService _vehicleService;

        public VehicleTypeController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }
    }
}
