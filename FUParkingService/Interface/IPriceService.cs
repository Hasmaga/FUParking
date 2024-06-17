using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPriceService
    {
        Task<Return<bool>> CreatePriceItemAsync(CreatePriceItemReqDto req);
        Task<Return<bool>> DeletePriceItemAsync(Guid id);
        Task<Return<IEnumerable<PriceItem>>> GetAllPriceItemByPriceTableAsync(Guid PriceTableId);
        Task<Return<bool>> CreatePriceTableAsync(CreatePriceTableReqDto req);
        Task<Return<bool>> UpdateStatusPriceTableAsync(ChangeStatusPriceTableReqDto req);
        Task<Return<IEnumerable<GetPriceTableResDto>>> GetAllPriceTableAsync();
    }
}
