using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class PriceTableService : IPriceTableService
    {
        private readonly IPriceTableRepository _priceTableRepository;

        public PriceTableService(IPriceTableRepository priceTableRepository)
        {
            _priceTableRepository = priceTableRepository;
        }
    }
}
