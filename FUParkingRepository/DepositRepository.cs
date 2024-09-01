using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class DepositRepository : IDepositRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public DepositRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Deposit>> CreateDepositAsync(Deposit deposit)
        {
            try
            {
                await _db.Deposits.AddAsync(deposit);
                await _db.SaveChangesAsync();
                return new Return<Deposit>
                {
                    Data = deposit,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Deposit>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Deposit>> GetDepositByAppTransIdAsync(string appTransId)
        {
            try
            {
                var result = await _db.Deposits.Where(d => d.AppTranId.Equals(appTransId)).FirstOrDefaultAsync();
                return new Return<Deposit>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<Deposit>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Deposit>> GetDepositByIdAsync(Guid depositId)
        {
            try
            {
                var result = await _db.Deposits.Where(d => d.Id.Equals(depositId)).FirstOrDefaultAsync();
                return new Return<Deposit>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<Deposit>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<Deposit>> UpdateDepositAppTranIdAsync(Guid id, string newTran)
        {
            try
            {
                var deposit = await _db.Deposits.FindAsync(id);
                if (deposit == null)
                {
                    return new Return<Deposit>
                    {
                        Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                    };
                }

                deposit.AppTranId = newTran;
                _db.Deposits.Update(deposit);
                await _db.SaveChangesAsync();

                return new Return<Deposit>
                {
                    Data = deposit,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Deposit>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

    }
}
