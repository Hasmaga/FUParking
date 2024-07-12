using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.Transaction;
using FUParkingModel.ResponseObject.Wallet;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IWalletService
    {        
        Task<Return<IEnumerable<GetInfoWalletTransResDto>>> GetTransactionWalletMainAsync(GetListObjectWithFillerDateReqDto req);
        Task<Return<IEnumerable<GetInfoWalletTransResDto>>> GetTransactionWalletExtraAsync(GetListObjectWithFillerDateReqDto req);
        Task<Return<int>> GetBalanceWalletMainAsync();
        Task<Return<GetWalletExtraResDto>> GetBalanceWalletExtraAsync();
    }
}
