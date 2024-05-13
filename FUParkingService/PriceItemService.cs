using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class PriceItemService : IPriceItemService
    {
        private readonly IPriceItemRepository _priceItemRepository;

        public PriceItemService(IPriceItemRepository priceItemRepository)
        {
            _priceItemRepository = priceItemRepository;
        }
    }
}
