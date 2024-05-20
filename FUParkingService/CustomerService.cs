using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using Microsoft.Extensions.FileProviders;
using System.Transactions;

namespace FUParkingService
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHelpperService _helpperService;
        private readonly IPackageRepository _packageRepository;
        private readonly IDepositRepository _depositRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;

        public CustomerService(ICustomerRepository customerRepository, IUserRepository userRepository, IHelpperService helpperService, IPackageRepository packageRepository, IDepositRepository depositRepository, ITransactionRepository transactionRepository, IWalletRepository walletRepository)
        {
            _customerRepository = customerRepository;
            _userRepository = userRepository;
            _helpperService = helpperService;
            _packageRepository = packageRepository;
            _depositRepository = depositRepository;
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
        }

        public async Task<Return<object>> BuyPackageAsync(BuyPackageReqDto req, Guid customerId)
        {
            Return<object> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                Return<Customer> customerRes = await _customerRepository.GetCustomerByIdAsync(customerId);
                if(customerRes.IsSuccess == false || customerRes.Data == null)
                {
                    throw new EntryPointNotFoundException(customerRes.Message);
                }

                Return<Package?> existPackageRes = await _packageRepository.GetPackageByPackageIdAsync(req.packageId);

                if (existPackageRes.Data == null)
                {
                    throw new EntryPointNotFoundException(existPackageRes.Message);
                }

                Deposit newDeposit = new()
                {
                    Name = existPackageRes.Data.Name,
                    PackageId = existPackageRes.Data.Id,
                    Description = DepositEnum.PACKAGE_DEPOSIT
                };
                Return<Deposit?> createDepositRes = await _depositRepository.CreateDepositAsync(newDeposit);
                if (createDepositRes.Data == null)
                {
                    throw new OperationCanceledException(createDepositRes.Message);
                }
                Return<Wallet?> cusWalletRes = await _walletRepository.GetWalletByCustomerId(customerRes.Data.Id);
                if (cusWalletRes.Data == null)
                {
                    throw new EntryPointNotFoundException(cusWalletRes.Message);
                }
                Wallet customerWallet = cusWalletRes.Data;
                FUParkingModel.Object.Transaction newTransaction = new()
                {
                    WalletId = customerWallet.Id,
                    DepositId = createDepositRes.Data.Id,
                    Amount = existPackageRes.Data.CoinAmount,
                    TransactionStatus = StatusTransactionEnum.SUCCEED,
                };
                var createTransactionRes = await _transactionRepository.CreateTransactionAsync(newTransaction);
                if (createTransactionRes.Data == null)
                {
                    throw new OperationCanceledException(createDepositRes.Message);
                }

                transaction.Complete();
                res.Message = SuccessfullyEnumServer.SUCCESSFULLY;
                res.IsSuccess = true;
                return res;
            }
            catch (Exception ex)
            {
                transaction.Dispose();
                // Help controller handle status codes
                if(ex is EntryPointNotFoundException)
                {
                    throw new EntryPointNotFoundException(ex.Message);
                }
                if(ex is OperationCanceledException)
                {
                    throw new OperationCanceledException(ex.Message);
                }
                res.InternalErrorMessage = ex.Message;
                return res;
            }
        }

        public async Task<Return<bool>> ChangeStatusCustomerAsync(ChangeStatusCustomerReqDto req)
        {
            try
            {
                // Check token 
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains((userlogged.Data.Role ?? new Role()).Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                // Check CustomerId is exist
                var isCustomerExist = await _customerRepository.GetCustomerByIdAsync(req.CustomerId);
                if (isCustomerExist.Data == null || isCustomerExist.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.CUSTOMER_NOT_EXIST
                    };
                }
                if (req.IsActive)
                {
                    if ((isCustomerExist.Data.StatusCustomer ?? "").Equals(StatusCustomerEnum.ACTIVE))
                    {
                        return new Return<bool>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    else
                    {
                        isCustomerExist.Data.StatusCustomer = StatusCustomerEnum.ACTIVE;
                        // Update status Account
                        var isUpdate = await _customerRepository.UpdateCustomerAsync(isCustomerExist.Data);
                        if (isUpdate.Data == null || isUpdate.IsSuccess == false)
                        {
                            return new Return<bool>
                            {
                                IsSuccess = false,
                                Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR
                            };
                        }
                        else
                        {
                            return new Return<bool> { IsSuccess = true, Data = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
                        }
                    }
                }
                else
                {
                    if ((isCustomerExist.Data.StatusCustomer ?? "").Equals(StatusCustomerEnum.INACTIVE))
                    {
                        return new Return<bool>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    else
                    {
                        isCustomerExist.Data.StatusCustomer = StatusCustomerEnum.ACTIVE;
                        // Update status Account
                        var isUpdate = await _customerRepository.UpdateCustomerAsync(isCustomerExist.Data);
                        if (isUpdate.Data == null || isUpdate.IsSuccess == false)
                        {
                            return new Return<bool>
                            {
                                IsSuccess = false,
                                Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR
                            };
                        }
                        else
                        {
                            return new Return<bool> { IsSuccess = true, Data = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex.Message
                };
            }
        }
    }
}
