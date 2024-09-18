using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IPriceRepository
    {
        Task<Return<PriceTable>> CreatePriceTableAsync(PriceTable priceTable);
        Task<Return<PriceTable>> GetPriceTableByIdAsync(Guid id);
        Task<Return<PriceTable>> UpdatePriceTableAsync(PriceTable priceTable);
        Task<Return<IEnumerable<PriceTable>>> GetAllPriceTableAsync(GetListObjectWithFiller req);
        Task<Return<PriceItem>> CreatePriceItemAsync(PriceItem priceItem);        
        Task<Return<PriceItem>> GetPriceItemByIdAsync(Guid id);
        Task<Return<IEnumerable<PriceItem>>> GetAllPriceItemByPriceTableAsync(Guid PriceTableId);
        Task<Return<IEnumerable<PriceTable>>> GetListPriceTableActiveByVehicleTypeAsync(Guid vehicleTypeId);
        Task<Return<PriceTable>> GetPriceTableByPriorityAndVehicleTypeAsync(int priority, Guid vehicleTypeId);
        Task<Return<PriceTable>> GetDefaultPriceTableByVehicleTypeAsync(Guid vehicleTypeId);
        Task<Return<IEnumerable<PriceItem>>> GetListOverlapPriceItemAsync(Guid priceTableId, int from, int to);
        Task<Return<PriceItem>> GetDefaultPriceItemByPriceTableIdAsync(Guid priceTableId);
        Task<Return<dynamic>> DeleteAllPriceItemByPriceTableIdAsync(Guid priceTableId);
        Task<Return<IEnumerable<PriceItem>>> GetAllPriceItemByPriceTableWithPageAsync(Guid PriceTableId, GetListObjectWithPageReqDto req);
        Task<Return<IEnumerable<PriceTable>>> GetAllPriceTableByVehicleTypeAsync(Guid vehicleType);
    }
}
