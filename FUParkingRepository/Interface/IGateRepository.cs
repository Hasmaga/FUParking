using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IGateRepository
    {
        Task<Return<Gate>> CreateGateAsync(Gate gate);
        Task<Return<IEnumerable<Gate>>> GetAllGateAsync();
        Task<Return<IEnumerable<GateType>>> GetAllGateTypeAsync();
        Task<Return<GateType>> CreateGateTypeAsync(GateType gateType);
        Task<Return<Gate>> UpdateGateAsync(Gate gate);
        Task<Return<Gate>> GetGateByIdAsync(Guid id);
    }
}
