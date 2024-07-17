using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.Transaction;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHelpperService _helpperService;

        public TransactionService(ITransactionRepository transactionRepository, IUserRepository userRepository, IHelpperService helpperService)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _helpperService = helpperService;
        }

        public async Task<Return<IEnumerable<GetTransactionPaymentResDto>>> GetListTransactionPaymentAsync(GetListObjectWithFillerAttributeAndDateReqDto req)
        {
            try
            {
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<IEnumerable<GetTransactionPaymentResDto>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!accountLogin.IsSuccess)
                {
                    return new Return<IEnumerable<GetTransactionPaymentResDto>>
                    {
                        InternalErrorMessage = accountLogin.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (!accountLogin.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || accountLogin.Data == null)
                {
                    return new Return<IEnumerable<GetTransactionPaymentResDto>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthSupervisor.Contains(accountLogin.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<GetTransactionPaymentResDto>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                var result = await _transactionRepository.GetListTransactionPaymentAsync(req.PageSize, req.PageIndex, req.StartDate, req.EndDate, req.SearchInput, req.Attribute);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetTransactionPaymentResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<IEnumerable<GetTransactionPaymentResDto>>
                {
                    Data = result.Data?.Select(p => new GetTransactionPaymentResDto
                    {
                        Amount = p.Amount,
                        CreateDateTime = p.CreatedDate,
                        Email = p.Wallet?.Customer?.Email ?? "",
                        PackageName = p.Deposit?.Package?.Name ?? "",
                        PaymentMethod = p.Payment?.PaymentMethod?.Name ?? "",
                        TransactionDescription = p.TransactionDescription,
                        TransactionStatus = p.TransactionStatus,
                        WalletType = p.Wallet?.WalletType ?? "",                       
                    }),
                    TotalRecord = result.TotalRecord,
                    IsSuccess = true,                   
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                };
            } 
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetTransactionPaymentResDto>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        //public async Task<Return<List<Transaction>>> GetTransactionsAsync(DateTime fromDate, DateTime toDate, int pageSize, int pageIndex, Guid userGuid)
        //{
        //    Return<List<Transaction>> res = new() { Message = ErrorEnumApplication.SERVER_ERROR };
        //    try
        //    {
        //        Return<User> userRes = await _userRepository.GetUserByIdAsync(userGuid);
        //        bool isStaff = userRes.Data?.Role?.Name?.ToLower().Equals(RoleEnum.STAFF.ToLower()) ?? false;
        //        if (userRes.Data == null || isStaff)
        //        {
        //            res.Message = ErrorEnumApplication.NOT_AUTHORITY;
        //            return res;
        //        }

        //        if ((userRes.Data.StatusUser ?? "").ToLower().Equals(StatusUserEnum.INACTIVE.ToLower()))
        //        {
        //            res.Message = ErrorEnumApplication.BANNED;
        //            return res;
        //        }

        //        if (fromDate > toDate)
        //        {
        //            res.Message = ErrorEnumApplication.DATE_OVERLAPSED;
        //            return res;
        //        }
        //        res.IsSuccess = true;
        //        res.Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY;
        //        res.Data = (await _transactionRepository.GetTransactionListAsync(fromDate, toDate, pageSize, pageIndex)).Data.ToList();
        //        return res;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}
    }
}
