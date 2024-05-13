using FUParkingService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/price-item")]
    public class PriceItemController : Controller
    {
        private readonly IPriceItemService _priceItemService;

        public PriceItemController(IPriceItemService priceItemService)
        {
            _priceItemService = priceItemService;
        }
    }
}
