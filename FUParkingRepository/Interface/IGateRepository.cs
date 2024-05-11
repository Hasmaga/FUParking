using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IGateRepository
    {
        Task<Return<Gate>> CreateGateAsync(Gate gate);
    }
}
