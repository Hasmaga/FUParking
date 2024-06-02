using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Customer;
using FUParkingModel.ResponseObject.Customer;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
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

        public async Task<Return<bool>> BuyPackageAsync(BuyPackageReqDto req, Guid customerId)
        {
            Return<bool> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                Return<Customer> customerRes = await _customerRepository.GetCustomerByIdAsync(customerId);
                if (customerRes.IsSuccess == false || customerRes.Data == null)
                {
                    res.Message = customerRes.Message;
                    return res;
                }

                if (!Guid.TryParse(req.packageId, out Guid packageGuid))
                {
                    transaction.Dispose();
                    res.Message = ErrorEnumApplication.PACKAGE_NOT_EXIST;
                    return res;
                }

                Return<Package?> existPackageRes = await _packageRepository.GetPackageByPackageIdAsync(packageGuid);

                if (existPackageRes.Data == null)
                {
                    transaction.Dispose();
                    res.Message = existPackageRes.Message;
                    return res;
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
                    transaction.Dispose();
                    res.Message = createDepositRes.Message;
                    return res;
                }
                Return<Wallet?> cusWalletRes = await _walletRepository.GetWalletByCustomerId(customerRes.Data.Id);
                if (cusWalletRes.Data == null)
                {
                    transaction.Dispose();
                    res.Message = cusWalletRes.Message;
                    return res;
                }
                Wallet customerWallet = cusWalletRes.Data;
                FUParkingModel.Object.Transaction newTransaction = new()
                {
                    WalletId = customerWallet.Id,
                    DepositId = createDepositRes.Data.Id,
                    Amount = existPackageRes.Data.CoinAmount,
                    TransactionStatus = StatusTransactionEnum.PENDING,
                };
                var createTransactionRes = await _transactionRepository.CreateTransactionAsync(newTransaction);
                if (createTransactionRes.Data == null)
                {
                    transaction.Dispose();
                    res.Message = createDepositRes.Message;
                    return res;
                }
                transaction.Complete();
                res.Message = SuccessfullyEnumServer.SUCCESSFULLY;
                res.IsSuccess = true;
                return res;
            }
            catch (Exception ex)
            {
                transaction.Dispose();
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
                if (!Auth.AuthManager.Contains((userlogged.Data.Role ?? new Role() { Name = RoleEnum.MANAGER }).Name))
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
                        isCustomerExist.Data.StatusCustomer = StatusCustomerEnum.INACTIVE;
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

        public async Task<Return<Customer>> CreateCustomerAsync(CustomerReqDto customerReq, Guid userGuid)
        {
            Return<Customer> res = new() { Message = ErrorEnumApplication.SERVER_ERROR };
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
                Return<Customer> foundCustomerRes = await _customerRepository.GetCustomerByEmailAsync(customerReq.Email);
                if(foundCustomerRes.Data != null)
                {
                    res.Message = ErrorEnumApplication.EMAIL_IS_EXIST;
                    return res;
                }

                Return<CustomerType> typeFreeRes = await _customerRepository.GetCustomerTypeByNameAsync(CustomerTypeEnum.FREE);
                if (typeFreeRes.Data == null)
                {
                    res.Message = typeFreeRes.Message;
                    return res;
                }
                Customer newCustomer = new() {
                    FullName = customerReq.Name,
                    StatusCustomer = StatusCustomerEnum.ACTIVE, 
                    CustomerTypeId = typeFreeRes.Data.Id, 
                    Email = customerReq.Email, 
                    Phone = customerReq.Phone,
                    PasswordHash = await _helpperService.CreatePassHashAndPassSaltAsync(customerReq.Password, out byte[] outPasswordSalt) };
                res = await _customerRepository.CreateNewCustomerAsync(newCustomer);
                return res;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Return<Customer>> GetCustomerByIdAsync(Guid customerId)
        {
            Return<Customer> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                Return<Customer> customerRes = await _customerRepository.GetCustomerByIdAsync(customerId);

                if (customerRes.Data == null)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }

                if ((customerRes.Data.StatusCustomer ?? "").ToLower().Equals(StatusCustomerEnum.INACTIVE.ToLower()))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }

                return customerRes;
            }
            catch (Exception)
            {
                return res;
            }
        }

        public async Task<Return<List<GetCustomersWithFillerResDto>>> GetListCustomerAsync(GetCustomersWithFillerReqDto req)
        {
            Return<List<GetCustomersWithFillerResDto>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false
            };
            try
            {
                if (!_helpperService.IsTokenValid())
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                var userLogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userLogged.Data == null || userLogged.IsSuccess == false)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }

                if (!Auth.AuthManager.Contains(userLogged.Data.Role?.Name ?? ""))
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }

                if (userLogged.Data.StatusUser.Equals(StatusUserEnum.INACTIVE))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }
                
                Return<List<Customer>> listCustomerRes = await _customerRepository.GetListCustomerAsync(req);                
                List<GetCustomersWithFillerResDto> listCustomer = new();
                foreach (var item in listCustomerRes.Data ?? new List<Customer>())
                {
                    listCustomer.Add(new GetCustomersWithFillerResDto
                    {
                        CustomerId = item.Id,
                        FullName = item.FullName ?? "",
                        Email = item.Email ?? "",
                        Phone = item.Phone ?? "",
                        StatusCustomer = item.StatusCustomer ?? "",
                        CustomerType = item.CustomerType?.Name ?? "",
                        CreateDate = DateOnly.FromDateTime(item.CreatedDate)
                    });
                }
                res.Data = listCustomer;
                res.TotalRecord = listCustomer.Count;
                res.Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY;
                res.IsSuccess = true;
                return res;
            }
            catch
            {
                throw;
            }
        }
    }
}
