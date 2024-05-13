using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class DepositService : IDepositService
    {
        private readonly IDepositRepository _depositRepository;

        public DepositService(IDepositRepository depositRepository)
        {
            _depositRepository = depositRepository;
        }
    }
}
