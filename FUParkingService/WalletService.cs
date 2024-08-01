using FUParkingModel.Enum;
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
        private readonly IHelpperService _helpperService;
        private readonly ITransactionRepository _transactionRepository;

        public WalletService(IWalletRepository walletRepository, IHelpperService helpperService, ITransactionRepository transactionRepository)
        {
            _walletRepository = walletRepository;
            _helpperService = helpperService;
            _transactionRepository = transactionRepository;
        }

        public async Task<Return<IEnumerable<GetInfoWalletTransResDto>>> GetTransactionWalletExtraAsync(GetListObjectWithFillerDateReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var wallet = await _walletRepository.GetExtraWalletByCustomerId(checkAuth.Data.Id);
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
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetInfoWalletTransResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var wallet = await _walletRepository.GetMainWalletByCustomerId(checkAuth.Data.Id);
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
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<int>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var wallet = await _walletRepository.GetMainWalletByCustomerId(checkAuth.Data.Id);
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
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<GetWalletExtraResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var wallet = await _walletRepository.GetExtraWalletByCustomerId(checkAuth.Data.Id);
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
