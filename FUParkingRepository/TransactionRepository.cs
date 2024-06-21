using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public TransactionRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Transaction>> CreateTransactionAsync(Transaction transaction)
        {
            try
            {
                await _db.Transactions.AddAsync(transaction);
                await _db.SaveChangesAsync();
                return new Return<Transaction>
                {
                    Data = transaction,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Transaction>
                {                    
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<Transaction>> GetTransactionByDepositIdAsync(Guid transactionId)
        {
            try
            {
                var result = await _db.Transactions.Include(t => t.Payment).FirstOrDefaultAsync(t => t.Id.Equals(transactionId));
                return new Return<Transaction>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<Transaction>
                {                    
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<List<Transaction>>> GetTransactionListAsync(DateTime FromDate, DateTime ToDate, int pageSize, int pageIndex)
        {
            Return<List<Transaction>> res = new() { Message = ErrorEnumApplication.GET_OBJECT_ERROR };
            try
            {
                res.Data = await _db.Transactions.Include(t => t.Payment).Where(t => t.CreatedDate >= FromDate && t.CreatedDate <= ToDate)
                                                                .OrderByDescending(t => t.CreatedDate)
                                                                .Skip((pageIndex - 1) * pageSize)
                                                                .Take(pageSize)
                                                                .ToListAsync();
                res.Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY;
                res.IsSuccess = true;
                return res;
            }
            catch
            {
                return res;
            }
        }

        public async Task<Return<bool>> UpdateTransactionAsync(Transaction transaction)
        {
            try
            {
                _db.Update(transaction);
                await _db.SaveChangesAsync();
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<bool>
                {                    
                    Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
