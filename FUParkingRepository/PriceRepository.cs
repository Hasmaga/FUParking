using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;
using System;

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

        public async Task<Return<IEnumerable<PriceTable>>> GetAllPriceTableAsync(GetListObjectWithFillerAttributeAndDateReqDto req)
        {
            try
            {
                var query = _db.PriceTables
                    .Include(r => r.VehicleType)
                    .Where(t => t.DeletedDate == null)
                    .AsQueryable();

                if (req.SearchInput != null && req.Attribute !=null)
                {
                    switch (req.Attribute.ToLower())
                    {
                        case "name":
                            query = query.Where(r => r.Name.Contains(req.SearchInput));
                            break;                        
                        case "vehicletype":
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                            query = query.Where(r => r.VehicleType.Name.Contains(req.SearchInput));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                            break;
                        default:
                            break;
                    }
                }
                req.StartDate ??= DateTime.MinValue;
                req.EndDate ??= TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                query = query.Where(r => r.ApplyFromDate >= req.StartDate && r.ApplyToDate <= req.EndDate && (r.ApplyToDate == null || r.ApplyFromDate == null));

                var result = await query.OrderByDescending(t => t.CreatedDate)
                    .Skip((req.PageIndex - 1) * req.PageSize)
                    .Take(req.PageSize)
                    .ToListAsync();
                return new Return<IEnumerable<PriceTable>>
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = query.Count(),
                    Message = query.Any() ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
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
                var result = await _db.PriceTables
                    .Where(r => r.DeletedDate == null && r.Id.Equals(id))
                    .FirstOrDefaultAsync();
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
                var datetimenow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var result = await _db.PriceTables
                    .Include(r => r.VehicleType)
                    .Where(t => t.DeletedDate == null &&
                        t.VehicleTypeId.Equals(vehicleTypeId) &&
                        (
                            (t.ApplyFromDate == null && t.ApplyToDate == null) || // Active forever
                            (t.ApplyFromDate != null && t.ApplyToDate == null && t.ApplyFromDate <= datetimenow || // Check ApplyFromDate
                            (t.ApplyFromDate == null && t.ApplyToDate != null && t.ApplyToDate >= datetimenow)) // Check ApplyToDate
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

        public async Task<Return<dynamic>> DeleteAllPriceItemByPriceTableIdAsync(Guid priceTableId)
        {
            try
            {
                var priceItems = await _db.PriceItems.Where(r => r.PriceTableId.Equals(priceTableId)).ToListAsync();
                foreach (var item in priceItems)
                {
                    _db.PriceItems.Remove(item);
                }
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
    }
}
