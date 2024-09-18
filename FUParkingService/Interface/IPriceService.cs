using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Price;
using FUParkingModel.ResponseObject;
using FUParkingModel.ResponseObject.Price;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPriceService
    {
        Task<Return<dynamic>> CreateDefaultPriceItemForDefaultPriceTableAsync(CreateDefaultItemPriceReqDto req);
        Task<Return<dynamic>> CreatePriceItemAsync(CreateListPriceItemReqDto req);
        Task<Return<IEnumerable<GetPriceItemResDto>>> GetAllPriceItemByPriceTableAsync(Guid PriceTableId, GetListObjectWithPageReqDto req);
        Task<Return<bool>> UpdatePriceItemAsync(CreateListPriceItemReqDto req);
        Task<Return<dynamic>> CreateDefaultPriceTableAsync(CreateDefaultPriceTableReqDto req);
        Task<Return<dynamic>> CreatePriceTableAsync(CreatePriceTableReqDto req);
        Task<Return<IEnumerable<GetPriceTableResDto>>> GetAllPriceTableAsync(GetListObjectWithFiller req);
        Task<Return<bool>> UpdateStatusPriceTableAsync(ChangeStatusPriceTableReqDto req);
        Task<Return<bool>> DeletePriceTableAsync(Guid pricetableId);
        Task<Return<dynamic>> UpdatePriceTableAsync(UpdatePriceTableReqDto req);
        Task<Return<IEnumerable<GetPriceTableResDto>>> GetAllPriceTableByVehicleTypeAsync(Guid vehicleTypeId);
    }
}
