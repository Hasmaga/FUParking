using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class PriceRepository : IPriceRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public PriceRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<PriceItem>> CreatePriceItemAsync(PriceItem priceItem)
        {
            try
            {
                await _db.PriceItems.AddAsync(priceItem);
                await _db.SaveChangesAsync();
                return new Return<PriceItem>
                {
                    Data = priceItem,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<PriceItem>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<dynamic>> DeletePriceItemAsync(PriceItem priceItem)
        {
            try
            {
                _db.PriceItems.Remove(priceItem);
                await _db.SaveChangesAsync();
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<PriceItem>>> GetAllPriceItemByPriceTableAsync(Guid PriceTableId)
        {
            try
            {
                var result = await _db.PriceItems.Where(r => r.PriceTableId.Equals(PriceTableId) && r.DeletedDate == null).ToListAsync();
                return new Return<IEnumerable<PriceItem>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = result.Count,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<PriceItem>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<PriceItem>> GetPriceItemByIdAsync(Guid id)
        {
            try
            {
                var result = await _db.PriceItems.Where(r => r.DeletedDate == null && r.Id.Equals(id)).FirstOrDefaultAsync();
                return new Return<PriceItem>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                };
            }
            catch (Exception e)
            {
                return new Return<PriceItem>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<PriceTable>> CreatePriceTableAsync(PriceTable priceTable)
        {
            try
            {
                await _db.PriceTables.AddAsync(priceTable);
                await _db.SaveChangesAsync();
                return new Return<PriceTable>
                {
                    Data = priceTable,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<PriceTable>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<PriceTable>>> GetAllPriceTableAsync()
        {
            try
            {
                var result = await _db.PriceTables.Include(r => r.VehicleType).Where(t => t.DeletedDate == null).ToListAsync();
                return new Return<IEnumerable<PriceTable>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = result.Count,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<PriceTable>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<PriceTable>> GetPriceTableByIdAsync(Guid id)
        {
            try
            {
                var result = await _db.PriceTables.Where(r => r.DeletedDate == null && r.Id.Equals(id)).FirstOrDefaultAsync();
                return new Return<PriceTable>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<PriceTable>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<PriceTable>> UpdatePriceTableAsync(PriceTable priceTable)
        {
            try
            {
                _db.PriceTables.Update(priceTable);
                await _db.SaveChangesAsync();
                return new Return<PriceTable>
                {
                    Data = priceTable,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<PriceTable>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<PriceTable>>> GetListPriceTableActiveByVehicleTypeAsync(Guid vehicleTypeId)
        {
            try
            {
                var result = await _db.PriceTables
                    .Include(r => r.VehicleType)
                    .Where(t => t.DeletedDate == null &&
                        t.VehicleTypeId.Equals(vehicleTypeId) &&
                        (
                            (t.ApplyFromDate == null && t.ApplyToDate == null) || // Active forever
                            (t.ApplyFromDate != null && t.ApplyToDate == null && t.ApplyFromDate <= DateTime.Now) || // Check ApplyFromDate
                            (t.ApplyFromDate == null && t.ApplyToDate != null && t.ApplyToDate >= DateTime.Now) // Check ApplyToDate
                        )
                    ).ToListAsync();
                return new Return<IEnumerable<PriceTable>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = result.Count,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<PriceTable>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<PriceTable>> GetPriceTableByPriorityAndVehicleTypeAsync(int priority, Guid vehicleTypeId)
        {
            try
            {
                var result = await _db.PriceTables
                    .Include(r => r.VehicleType)
                    .Where(t => t.DeletedDate == null &&
                        t.VehicleTypeId.Equals(vehicleTypeId) &&
                        t.Priority == priority
                    ).FirstOrDefaultAsync();
                return new Return<PriceTable>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<PriceTable>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<PriceTable>> GetDefaultPriceTableByVehicleTypeAsync(Guid vehicleTypeId)
        {
            try
            {
                var result = await _db.PriceTables
                    .Include(r => r.VehicleType)
                    .Where(t => t.DeletedDate == null &&
                        t.VehicleTypeId.Equals(vehicleTypeId) &&
                        t.Priority == 1
                    ).FirstOrDefaultAsync();
                return new Return<PriceTable>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<PriceTable>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<PriceItem>>> GetListOverlapPriceItemAsync(Guid priceTableId, int from, int to)
        {
            try
            {
                var result = await _db.PriceItems
                    .Where(r => r.DeletedDate == null &&
                        r.PriceTableId.Equals(priceTableId) &&
                        (
                            (r.ApplyFromHour >= from && r.ApplyFromHour <= to) ||
                            (r.ApplyToHour >= from && r.ApplyToHour <= to) ||
                            (r.ApplyFromHour <= from && r.ApplyToHour >= to)
                        )
                    ).ToListAsync();
                return new Return<IEnumerable<PriceItem>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = result.Count,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<PriceItem>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
                
            }
        }

        public async Task<Return<PriceItem>> GetDefaultPriceItemByPriceTableIdAsync(Guid priceTableId)
        {
            try
            {
                var result = await _db.PriceItems
                    .Where(r => r.DeletedDate == null &&
                        r.PriceTableId.Equals(priceTableId) &&
                        r.ApplyFromHour == null &&
                        r.ApplyToHour == null
                    ).FirstOrDefaultAsync();
                return new Return<PriceItem>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result != null ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<PriceItem>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }
    }
}
