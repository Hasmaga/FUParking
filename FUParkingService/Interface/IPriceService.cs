using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Price;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPriceService
    {
        Task<Return<dynamic>> CreateDefaultPriceItemForDefaultPriceTableAsync(CreateDefaultItemPriceReqDto req);
        Task<Return<dynamic>> CreatePriceItemAsync(CreateListPriceItemReqDto req);
        Task<Return<IEnumerable<PriceItem>>> GetAllPriceItemByPriceTableAsync(Guid PriceTableId);
        Task<Return<bool>> UpdatePriceItemAsync(CreateListPriceItemReqDto req);
        Task<Return<dynamic>> CreateDefaultPriceTableAsync(CreateDefaultPriceTableReqDto req);
        Task<Return<dynamic>> CreatePriceTableAsync(CreatePriceTableReqDto req);
        Task<Return<IEnumerable<GetPriceTableResDto>>> GetAllPriceTableAsync(GetListObjectWithFillerAttributeAndDateReqDto req);
        Task<Return<bool>> UpdateStatusPriceTableAsync(ChangeStatusPriceTableReqDto req);
        Task<Return<bool>> DeletePriceTableAsync(Guid pricetableId);
    }
}
