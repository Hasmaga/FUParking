using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.Transaction;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.ComponentModel;

namespace FUParkingService
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IHelpperService _helpperService;
        private readonly ITransactionRepository _transactionRepository;

        public WalletService(IWalletRepository walletRepository, ICustomerRepository customerRepository, IHelpperService helpperService, ITransactionRepository transactionRepository)
        {
            _walletRepository = walletRepository;
            _customerRepository = customerRepository;
            _helpperService = helpperService;
            _transactionRepository = transactionRepository;
        }

        public async Task<Return<GetWalletTransResDto>> GetTransactionWalletExtraAsync(GetListObjectWithFillerDateReqDto req)
        {
            try
            {
                var isTokenValid = _helpperService.IsTokenValid();
                if (!isTokenValid)
                {
                    return new Return<GetWalletTransResDto> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                var customerLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!customerLogged.IsSuccess)
                {
                    return new Return<GetWalletTransResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customerLogged.InternalErrorMessage };
                }
                if (!customerLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customerLogged.Data is null)
                {
                    return new Return<GetWalletTransResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var wallet = await _walletRepository.GetExtraWalletByCustomerId(customerLogged.Data.Id);
                if (!wallet.IsSuccess)
                {
                    return new Return<GetWalletTransResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = wallet.InternalErrorMessage };
                }
                if (!wallet.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || wallet.Data is null)
                {
                    return new Return<GetWalletTransResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var transactions = await _transactionRepository.GetTransByWalletIdAsync(wallet.Data.Id, req.PageSize, req.PageIndex, req.StartDate, req.EndDate);
                if (!transactions.IsSuccess)
                {
                    return new Return<GetWalletTransResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = transactions.InternalErrorMessage };
                }

                var data = transactions.Data?.Select(x => new GetInfoWalletTransResDto
                {
                    Amount = x.Amount,
                    TransactionDescription = x.TransactionDescription,
                    TransactionStatus = x.TransactionStatus,
                    Date = x.CreatedDate,
                    TransactionType = x.DepositId.HasValue ? TransactionTypeEnum.IN : TransactionTypeEnum.OUT
                });

                return new Return<GetWalletTransResDto>()
                {
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data = new GetWalletTransResDto { Transactions = data, Balance = wallet.Data.Balance },
                    TotalRecord = transactions.TotalRecord,
                };                
            } 
            catch(Exception ex)
            {
                return new Return<GetWalletTransResDto>()
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<GetWalletTransResDto>> GetTransactionWalletMainAsync(GetListObjectWithFillerDateReqDto req)
        {
            try
            {
                var isTokenValid = _helpperService.IsTokenValid();
                if (!isTokenValid)
                {
                    return new Return<GetWalletTransResDto> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                var customerLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!customerLogged.IsSuccess)
                {
                    return new Return<GetWalletTransResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customerLogged.InternalErrorMessage };
                }
                if (!customerLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customerLogged.Data is null)
                {
                    return new Return<GetWalletTransResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var wallet = await _walletRepository.GetMainWalletByCustomerId(customerLogged.Data.Id);
                if (!wallet.IsSuccess)
                {
                    return new Return<GetWalletTransResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = wallet.InternalErrorMessage };
                }
                if (!wallet.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || wallet.Data is null)
                {
                    return new Return<GetWalletTransResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var transactions = await _transactionRepository.GetTransByWalletIdAsync(wallet.Data.Id, req.PageSize, req.PageIndex, req.StartDate, req.EndDate);
                if (!transactions.IsSuccess)
                {
                    return new Return<GetWalletTransResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = transactions.InternalErrorMessage };
                }

                var data = transactions.Data?.Select(x => new GetInfoWalletTransResDto
                {
                    Amount = x.Amount,
                    TransactionDescription = x.TransactionDescription,
                    TransactionStatus = x.TransactionStatus,
                    Date = x.CreatedDate,
                    TransactionType = x.DepositId.HasValue ? TransactionTypeEnum.IN : TransactionTypeEnum.OUT
                });

                return new Return<GetWalletTransResDto>()
                {
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data = new GetWalletTransResDto { Transactions = data, Balance = wallet.Data.Balance },
                    TotalRecord = transactions.TotalRecord
                };
            }
            catch (Exception ex)
            {
                return new Return<GetWalletTransResDto>()
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<List<Transaction>>> GetWalletTransactionByCustomerIdAsync(string? customerId, int pageIndex, int pageSize, int numberOfDays)
        {
            Return<List<Transaction>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            DateTime now = DateTime.Now;
            DateTime lastDate = now.AddDays(-numberOfDays);

            try
            {
                if (customerId == null)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                Guid customerGuid = new(customerId);
                Return<Customer> customerRes = await _customerRepository.GetCustomerByIdAsync(customerGuid);
                if (customerRes.Data == null)
                {
                    res.Message = customerRes.Message;
                    return res;
                }

                Return<Wallet> foundWalletRes = await _walletRepository.GetWalletByCustomerId(customerGuid);
                if (foundWalletRes.Data == null)
                {
                    res.Message = foundWalletRes.Message;
                    return res;
                }
                Return<IEnumerable<Transaction>> transactionRes = await _walletRepository.GetWalletTransactionByWalletIdAsync(foundWalletRes.Data.Id, pageIndex, pageSize, now, lastDate);
                if (transactionRes.Data == null && !transactionRes.IsSuccess)
                {
                    res.Message = transactionRes.Message;
                    return res;
                }
                res.Message = transactionRes.Message;
                res.IsSuccess = true;
                res.Data = transactionRes.Data.ToList();
                return res;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
