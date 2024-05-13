using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class GateTypeService : IGateTypeService
    {
        private readonly IGateTypeRepository _gateTypeRepository;

        public GateTypeService(IGateTypeRepository gateTypeRepository)
        {
            _gateTypeRepository = gateTypeRepository;
        }
    }
}
