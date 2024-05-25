using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;

        public TransactionService(ITransactionRepository transactionRepository, IUserRepository userRepository)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
        }

        public async Task<Return<List<Transaction>>> GetTransactionsAsync(DateTime fromDate, DateTime toDate, int pageSize, int pageIndex, Guid userGuid)
        {
            Return<List<Transaction>> res = new() { Message = ErrorEnumApplication.SERVER_ERROR };
            try
            {
                Return<User> userRes = await _userRepository.GetUserByIdAsync(userGuid);
                bool isStaff = userRes.Data?.Role?.Name.ToLower().Equals(RoleEnum.STAFF.ToLower()) ?? false;
                if (userRes.Data == null || isStaff)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }

                if (userRes.Data.StatusUser.ToLower().Equals(StatusUserEnum.INACTIVE.ToLower()))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }

                if(fromDate > toDate)
                {
                    res.Message = ErrorEnumApplication.DATE_OVERLAPSED;
                    return res;
                }

                res = await _transactionRepository.GetTransactionListAsync(fromDate, toDate, pageSize, pageIndex);
                return res;
            }
            catch
            {
                throw;
            }
        }
    }
}
