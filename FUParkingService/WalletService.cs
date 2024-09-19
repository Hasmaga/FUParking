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
        private readonly ICustomerRepository _customerRepository;

        public WalletService(IWalletRepository walletRepository, IHelpperService helpperService, ITransactionRepository transactionRepository, ICustomerRepository customerRepository)
        {
            _walletRepository = walletRepository;
            _helpperService = helpperService;
            _transactionRepository = transactionRepository;
            _customerRepository = customerRepository;
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
                    Id = x.Id,
                    Amount = x.Amount,
                    TransactionDescription = x.TransactionDescription,
                    TransactionStatus = x.TransactionStatus,
                    Date = x.CreatedDate,
                    TransactionType = (x.DepositId is not null) ? TransactionTypeEnum.IN : TransactionTypeEnum.OUT
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
                    Id = x.Id,
                    Amount = x.Amount,
                    TransactionDescription = x.TransactionDescription,
                    TransactionStatus = x.TransactionStatus,
                    Date = x.CreatedDate,
                    TransactionType = (x.DepositId is not null || x.UserTopUpId is not null) ? TransactionTypeEnum.IN : TransactionTypeEnum.OUT
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

        public async Task<Return<GetBalanceWalletMainExtraResDto>> GetBalanceWalletMainExtraAsync(Guid customerId)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<GetBalanceWalletMainExtraResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                // check customer
                var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
                if (!customer.IsSuccess)
                {
                    return new Return<GetBalanceWalletMainExtraResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customer.InternalErrorMessage };
                }
                if (!customer.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customer.Data is null)
                {
                    return new Return<GetBalanceWalletMainExtraResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                // Check customerType
                if (customer.Data.CustomerType?.Name != CustomerTypeEnum.PAID)
                {
                    return new Return<GetBalanceWalletMainExtraResDto> { Message = ErrorEnumApplication.MUST_BE_CUSTOMER_PAID };
                }
                var walletMain = await _walletRepository.GetMainWalletByCustomerId(customerId);
                if (!walletMain.IsSuccess)
                {
                    return new Return<GetBalanceWalletMainExtraResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = walletMain.InternalErrorMessage };
                }
                if (!walletMain.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || walletMain.Data is null)
                {
                    return new Return<GetBalanceWalletMainExtraResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var walletExtra = await _walletRepository.GetExtraWalletByCustomerId(customerId);
                if (!walletExtra.IsSuccess)
                {
                    return new Return<GetBalanceWalletMainExtraResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = walletExtra.InternalErrorMessage };
                }
                if (!walletExtra.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || walletExtra.Data is null)
                {
                    return new Return<GetBalanceWalletMainExtraResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                return new Return<GetBalanceWalletMainExtraResDto>()
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data = new GetBalanceWalletMainExtraResDto
                    {
                        BalanceMain = walletMain.Data.Balance,
                        BalanceExtra = walletExtra.Data.Balance,
                        ExpDate = walletExtra.Data.EXPDate
                    }
                };
            }
            catch (Exception ex)
            {
                return new Return<GetBalanceWalletMainExtraResDto>()
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
