using FUParkingService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/vehicle-types")]
    public class VehicleTypeController : Controller
    {
        private readonly IVehicleService _vehicleService;

        public VehicleTypeController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }
    }
}
