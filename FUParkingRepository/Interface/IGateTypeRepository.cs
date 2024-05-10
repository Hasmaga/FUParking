using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IGateTypeRepository
    {
        Task<Return<IEnumerable<GateType>>> GetAllGateTypeAsync();
        Task<Return<GateType>> CreateGateTypeAsync(GateType gateType);
    }
}
