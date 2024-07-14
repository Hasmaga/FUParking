using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Price;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPriceService
    {
        Task<Return<dynamic>> CreatePriceItemAsync(CreateListPriceItemReqDto req);
        Task<Return<bool>> DeletePriceItemAsync(Guid id);
        Task<Return<IEnumerable<PriceItem>>> GetAllPriceItemByPriceTableAsync(Guid PriceTableId);
        Task<Return<dynamic>> CreatePriceTableAsync(CreatePriceTableReqDto req);
        Task<Return<bool>> UpdateStatusPriceTableAsync(ChangeStatusPriceTableReqDto req);
        Task<Return<IEnumerable<GetPriceTableResDto>>> GetAllPriceTableAsync();
        Task<Return<dynamic>> CreateDefaultPriceTableAsync(CreateDefaultPriceTableReqDto req);
        Task<Return<dynamic>> CreateDefaultPriceItemForDefaultPriceTableAsync(CreateDefaultItemPriceReqDto req);
    }
}
