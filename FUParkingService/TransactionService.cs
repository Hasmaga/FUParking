﻿using FirebaseService;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Firebase;
using FUParkingModel.RequestObject.Transaction;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ResponseObject.Transaction;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Helper;
using FUParkingService.Interface;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace FUParkingService
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHelpperService _helpperService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ILogger<TransactionService> _logger;
        private readonly IFirebaseService _firebaseService;

        public TransactionService(ITransactionRepository transactionRepository, IHelpperService helpperService, ICustomerRepository customerRepository, IWalletRepository walletRepository, ILogger<TransactionService> logger, IFirebaseService firebaseService)
        {
            _transactionRepository = transactionRepository;
            _helpperService = helpperService;
            _customerRepository = customerRepository;
            _walletRepository = walletRepository;
            _firebaseService = firebaseService;
            _logger = logger;
        }

        public async Task<Return<IEnumerable<GetTransactionPaymentResDto>>> GetListTransactionPaymentAsync(GetListObjectWithFillerAttributeAndDateReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetTransactionPaymentResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
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
                        Id = p.Id,
                        Amount = p.Amount,
                        CreatedDate = p.CreatedDate,
                        Email = p.Wallet?.Customer?.Email ?? "",
                        PackageName = p.Deposit?.Package?.Name ?? "",
                        PaymentMethod = p.Payment?.PaymentMethod?.Name ?? "",
                        TransactionDescription = p.TransactionDescription,
                        TransactionStatus = p.TransactionStatus,
                        WalletType = p.Wallet?.WalletType ?? "",
                    }),
                    TotalRecord = result.TotalRecord,
                    IsSuccess = true,
                    Message = result.TotalRecord > 0 ? SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY : ErrorEnumApplication.NOT_FOUND_OBJECT
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

        public async Task<Return<int>> GetRevenueTodayAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<int>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _transactionRepository.GetRevenueTodayAsync();
                if (!result.IsSuccess)
                {
                    return new Return<int>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<int>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<int>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<StatisticParkingAreaRevenueResDto>>> GetListStatisticParkingAreaRevenueAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<StatisticParkingAreaRevenueResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _transactionRepository.GetListStatisticParkingAreaRevenueAsync();
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<StatisticParkingAreaRevenueResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<StatisticParkingAreaRevenueResDto>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<StatisticRevenueParkingAreasDetailsResDto>>> GetListStatisticRevenueParkingAreasDetailsAsync(GetListObjectWithFillerDateAndSearchInputResDto req)
        {
            try
            {
                // Check auth 
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess)
                {
                    return new Return<IEnumerable<StatisticRevenueParkingAreasDetailsResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _transactionRepository.GetListStatisticRevenueParkingAreasDetailsAsync(req);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<StatisticRevenueParkingAreasDetailsResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }                
                return result;
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<StatisticRevenueParkingAreasDetailsResDto>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> TopUpCustomerWalletByUserAsync(TopUpCustomerWalletByUserReqDto req)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                // check customerId 
                var customer = await _customerRepository.GetCustomerByIdAsync(req.CustomerId);
                if (!customer.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = customer.InternalErrorMessage,
                        Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    };
                }

                // get wallet main
                var wallet = await _walletRepository.GetMainWalletByCustomerId(req.CustomerId);
                if (!wallet.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || wallet.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = wallet.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR,
                    };
                }

                wallet.Data.Balance += req.Amount;
                var resultUpdateWallet = await _walletRepository.UpdateWalletAsync(wallet.Data);
                if (!resultUpdateWallet.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = resultUpdateWallet.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR,
                    };
                }

                var transaction = new FUParkingModel.Object.Transaction
                {
                    Amount = req.Amount,
                    CreatedDate = DateTime.Now,
                    WalletId = wallet.Data.Id,
                    TransactionDescription = "Top Up Wallet By Staff",
                    TransactionStatus = StatusTransactionEnum.SUCCEED,
                    UserTopUpId = checkAuth.Data.Id,                    
                };

                var result = await _transactionRepository.CreateTransactionAsync(transaction);
                if (!result.IsSuccess)
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR,
                    };
                }

                // Firebase send notification
                if (!string.IsNullOrEmpty(customer.Data?.FCMToken))
                {
                    string formatedMoney = Utilities.FormatMoney(req.Amount);

                    var firebaseReq = new FirebaseReqDto
                    {
                        ClientTokens = new List<string> { customer.Data.FCMToken },
                        Title = "Top-up Successful",
                        Body = $"Dear {customer.Data?.FullName}, your wallet has been successfully topped up with {formatedMoney} coin.\n" +
                               $"\u2022 New Balance: {wallet.Data?.Balance:C}\n" +
                               $"\u2022 Top-up processed by: {checkAuth.Data?.FullName}\n" +
                               $"\u2022 Date: {Utilities.FormatDateTime(DateTime.Now)}"
                    };

                    var notificationResult = await _firebaseService.SendNotificationAsync(firebaseReq);
                    if (!notificationResult.IsSuccess)
                    {
                        _logger.LogError("Failed to send notification to customer Id {CustomerId}. Error: {Error}", customer.Data?.Id, notificationResult.InternalErrorMessage);
                    }
                }


                scope.Complete();
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                };
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<StatisticRevenueOfParkingSystemResDto>>> GetListStatisticRevenueOfParkingSystemAsync(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<StatisticRevenueOfParkingSystemResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _transactionRepository.GetListStatisticRevenueOfParkingSystemAsync(startDate, endDate);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<StatisticRevenueOfParkingSystemResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<StatisticRevenueOfParkingSystemResDto>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<StatisticRevenueOfParkingAreaSystemDetailResDto>>> GetListStatisticRevenueOfParkingSystemDetailsAsync(Guid parkingId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<StatisticRevenueOfParkingAreaSystemDetailResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _transactionRepository.GetListStatisticRevenueOfParkingSystemDetailsAsync(parkingId, startDate, endDate);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<StatisticRevenueOfParkingAreaSystemDetailResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<StatisticRevenueOfParkingAreaSystemDetailResDto>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
