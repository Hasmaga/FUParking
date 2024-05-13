using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class GateService : IGateService
    {
        private readonly ICardRepository _cardRepository;

        public GateService(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
        }
    }
}
