﻿using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IPriceTableRepository
    {
        Task<Return<PriceTable>> CreatePriceTableAsync(PriceTable priceTable);
        Task<Return<PriceTable>> GetPriceTableByIdAsync(Guid id);
        Task<Return<PriceTable>> UpdatePriceTableAsync(PriceTable priceTable);
        Task<Return<IEnumerable<PriceTable>>> GetAllPriceTableAsync();
    }
}
