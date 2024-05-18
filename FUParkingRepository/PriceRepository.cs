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
                    IsSuccess = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<bool>> DeletePriceItemAsync(PriceItem priceItem)
        {
            try
            {
                _db.PriceItems.Remove(priceItem);
                await _db.SaveChangesAsync();
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.DELETE_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<IEnumerable<PriceItem>>> GetAllPriceItemByPriceTableAsync(Guid PriceTableId)
        {
            try
            {
                return new Return<IEnumerable<PriceItem>>
                {
                    Data = await _db.PriceItems.Where(r => r.PriceTableId == PriceTableId).ToListAsync(),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<PriceItem>> GetPriceItemByIdAsync(Guid id)
        {
            try
            {
                return new Return<PriceItem>
                {
                    Data = await _db.PriceItems.FindAsync(id),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<PriceItem>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
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
                    IsSuccess = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<IEnumerable<PriceTable>>> GetAllPriceTableAsync()
        {
            try
            {
                return new Return<IEnumerable<PriceTable>>
                {
                    Data = await _db.PriceTables.Include(r => r.VehicleType).ToListAsync(),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<PriceTable>> GetPriceTableByIdAsync(Guid id)
        {
            try
            {
                return new Return<PriceTable>
                {
                    Data = await _db.PriceTables.FindAsync(id),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<PriceTable>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
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
                    IsSuccess = false,
                    Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
