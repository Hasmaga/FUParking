﻿using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingRepository.Interface
{
    public interface IPriceRepository
    {
        Task<Return<PriceTable>> CreatePriceTableAsync(PriceTable priceTable);
        Task<Return<PriceTable>> GetPriceTableByIdAsync(Guid id);
        Task<Return<PriceTable>> UpdatePriceTableAsync(PriceTable priceTable);
        Task<Return<IEnumerable<PriceTable>>> GetAllPriceTableAsync();
        Task<Return<PriceItem>> CreatePriceItemAsync(PriceItem priceItem);
        Task<Return<bool>> DeletePriceItemAsync(PriceItem priceItem);
        Task<Return<PriceItem>> GetPriceItemByIdAsync(Guid id);
        Task<Return<IEnumerable<PriceItem>>> GetAllPriceItemByPriceTableAsync(Guid PriceTableId);
    }
}