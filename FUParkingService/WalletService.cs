using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.Transaction;
using FUParkingModel.ResponseObject.Wallet;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

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

        public async Task<Return<IEnumerable<GetInfoWalletTransResDto>>> GetTransactionWalletExtraAsync(GetListObjectWithFillerDateReqDto req)
        {
            try
            {
                var isTokenValid = _helpperService.IsTokenValid();
                if (!isTokenValid)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                var customerLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!customerLogged.IsSuccess)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customerLogged.InternalErrorMessage };
                }
                if (!customerLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customerLogged.Data is null)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var wallet = await _walletRepository.GetExtraWalletByCustomerId(customerLogged.Data.Id);
                if (!wallet.IsSuccess)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = wallet.InternalErrorMessage };
                }
                if (!wallet.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || wallet.Data is null)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var transactions = await _transactionRepository.GetTransByWalletIdAsync(wallet.Data.Id, req.PageSize, req.PageIndex, req.StartDate, req.EndDate);
                if (!transactions.IsSuccess)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = transactions.InternalErrorMessage };
                }

                var data = transactions.Data?.Select(x => new GetInfoWalletTransResDto
                {
                    Amount = x.Amount,
                    TransactionDescription = x.TransactionDescription,
                    TransactionStatus = x.TransactionStatus,
                    Date = x.CreatedDate,
                    TransactionType = x.DepositId.HasValue ? TransactionTypeEnum.IN : TransactionTypeEnum.OUT
                });

                return new Return<IEnumerable<GetInfoWalletTransResDto>>()
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data = data,
                    TotalRecord = transactions.TotalRecord,
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetInfoWalletTransResDto>>()
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<GetInfoWalletTransResDto>>> GetTransactionWalletMainAsync(GetListObjectWithFillerDateReqDto req)
        {
            try
            {
                var isTokenValid = _helpperService.IsTokenValid();
                if (!isTokenValid)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                var customerLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!customerLogged.IsSuccess)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customerLogged.InternalErrorMessage };
                }
                if (!customerLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customerLogged.Data is null)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var wallet = await _walletRepository.GetMainWalletByCustomerId(customerLogged.Data.Id);
                if (!wallet.IsSuccess)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = wallet.InternalErrorMessage };
                }
                if (!wallet.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || wallet.Data is null)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var transactions = await _transactionRepository.GetTransByWalletIdAsync(wallet.Data.Id, req.PageSize, req.PageIndex, req.StartDate, req.EndDate);
                if (!transactions.IsSuccess)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = transactions.InternalErrorMessage };
                }

                var data = transactions.Data?.Select(x => new GetInfoWalletTransResDto
                {
                    Amount = x.Amount,
                    TransactionDescription = x.TransactionDescription,
                    TransactionStatus = x.TransactionStatus,
                    Date = x.CreatedDate,
                    TransactionType = x.DepositId.HasValue ? TransactionTypeEnum.IN : TransactionTypeEnum.OUT
                });

                return new Return<IEnumerable<GetInfoWalletTransResDto>>()
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data = data,
                    TotalRecord = transactions.TotalRecord
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetInfoWalletTransResDto>>()
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<int>> GetBalanceWalletMainAsync()
        {
            try
            {
                var isTokenValid = _helpperService.IsTokenValid();
                if (!isTokenValid)
                {
                    return new Return<int> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                var customerLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!customerLogged.IsSuccess)
                {
                    return new Return<int> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customerLogged.InternalErrorMessage };
                }
                if (!customerLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customerLogged.Data is null)
                {
                    return new Return<int> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var wallet = await _walletRepository.GetMainWalletByCustomerId(customerLogged.Data.Id);
                if (!wallet.IsSuccess)
                {
                    return new Return<int> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = wallet.InternalErrorMessage };
                }
                if (!wallet.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || wallet.Data is null)
                {
                    return new Return<int> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                return new Return<int>()
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data = wallet.Data.Balance
                };
            }
            catch (Exception ex)
            {
                return new Return<int>()
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<GetWalletExtraResDto>> GetBalanceWalletExtraAsync()
        {
            try
            {
                var isTokenValid = _helpperService.IsTokenValid();
                if (!isTokenValid)
                {
                    return new Return<GetWalletExtraResDto> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                var customerLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!customerLogged.IsSuccess)
                {
                    return new Return<GetWalletExtraResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customerLogged.InternalErrorMessage };
                }
                if (!customerLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customerLogged.Data is null)
                {
                    return new Return<GetWalletExtraResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var wallet = await _walletRepository.GetExtraWalletByCustomerId(customerLogged.Data.Id);
                if (!wallet.IsSuccess)
                {
                    return new Return<GetWalletExtraResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = wallet.InternalErrorMessage };
                }
                if (!wallet.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || wallet.Data is null)
                {
                    return new Return<GetWalletExtraResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                return new Return<GetWalletExtraResDto>()
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data = new GetWalletExtraResDto
                    {
                        Balance = wallet.Data.Balance,
                        ExpiredDate = wallet.Data.EXPDate
                    }
                };
            }
            catch (Exception ex)
            {
                return new Return<GetWalletExtraResDto>()
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
