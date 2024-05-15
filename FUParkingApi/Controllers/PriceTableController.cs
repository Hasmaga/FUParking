using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/price-tables")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class PriceTableController : Controller
    {
        private readonly IPriceTableService _priceTableService;

        public PriceTableController(IPriceTableService priceTableService)
        {
            _priceTableService = priceTableService;
        }
    }
}
