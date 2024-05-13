using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class ParkingAreaService : IParkingAreaService
    {
        private readonly IParkingAreaRepository _parkingAreaRepository;

        public ParkingAreaService(IParkingAreaRepository parkingAreaRepository)
        {
            _parkingAreaRepository = parkingAreaRepository;
        }
    }
}
