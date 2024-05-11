﻿using FUParkingModel.DatabaseContext;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    public class PriceTableRepository : IPriceTableRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public PriceTableRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public Task<Return<PriceTable>> CreatePriceTableAsync(PriceTable priceTable)
        {
            throw new NotImplementedException();
        }
    }
}
