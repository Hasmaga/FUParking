using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IDepositRepository
    {
        Task<Return<Deposit>> CreateDepositAsync(Deposit deposit);
        Task<Return<Deposit>> GetDepositByAppTransIdAsync(string appTransId);
    }
}
