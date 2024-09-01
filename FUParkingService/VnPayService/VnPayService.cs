using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Zalo;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FUParkingService.VnPayService
{
    public class VnPayService : IVnPayService
    {
        private readonly IHelpperService _helperService;
        private readonly IConfiguration _configuration;
        private readonly IPackageRepository _packageRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IDepositRepository _depositRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;

        public VnPayService(
            IPackageRepository packageRepository,
            IConfiguration configuration,
            IHelpperService helperService,
            IPaymentRepository paymentRepository,
            IDepositRepository depositRepository,
            ITransactionRepository transactionRepository,
            IWalletRepository walletRepository)
        {
            _configuration = configuration;
            _packageRepository = packageRepository;
            _helperService = helperService;
            _paymentRepository = paymentRepository;
            _depositRepository = depositRepository;
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
        }

        public async Task<Return<dynamic>> CustomerCreateRequestBuyPackageByVnPayAsync(Guid packageId, IPAddress ipAddress)
        {
            var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var checkAuth = await _helperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var package = await _packageRepository.GetPackageByPackageIdAsync(packageId);
                if (package.IsSuccess == false || package.Data == null)
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.PACKAGE_NOT_EXIST };
                }

                // create deposit
                var paymentMethod = (await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.VNPAY)).Data;
                if (paymentMethod == null)
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                Deposit newDeposit = new()
                {
                    PaymentMethodId = paymentMethod.Id,
                    PackageId = package.Data.Id,
                    CustomerId = checkAuth.Data.Id,
                    Amount = package.Data.Price,
                };

                var deposit = await _depositRepository.CreateDepositAsync(newDeposit);
                if (deposit.IsSuccess == false || deposit.Data == null)
                {
                    scope.Dispose();
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                string? vnp_TmnCode = _configuration.GetSection("VnPay:vnp_TmnCode").Value;
                string? vnp_ReturnUrl = _configuration.GetSection("VnPay:vnp_ReturnUrl").Value;
                string? vnp_Url = _configuration.GetSection("VnPay:vnp_Url").Value;
                string? vnp_HashSecret = _configuration.GetSection("VnPay:vnp_HashSecret").Value;

                var vnpay = new VnPayLibrary();
                vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode ?? "");
                vnpay.AddRequestData("vnp_Amount", (package.Data.Price * 100).ToString());
                vnpay.AddRequestData("vnp_CreateDate", now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", ipAddress.ToString());
                vnpay.AddRequestData("vnp_OrderInfo", $"Buy package {package.Data.Name} - Bai Parking");
                vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl ?? "");
                vnpay.AddRequestData("vnp_TxnRef", deposit.Data.Id.ToString());
                vnpay.AddRequestData("vnp_Locale", "en");
                vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

                // Create secure hash and get payment URL
                string paymentUrl = vnpay.CreateRequestUrl(vnp_Url ?? "", vnp_HashSecret ?? "");

                // Create transaction
                FUParkingModel.Object.Transaction newTransaction = new()
                {
                    Amount = package.Data.Price,
                    TransactionDescription = $"Buy package {package.Data.Name} - Bai Parking",
                    TransactionStatus = StatusTransactionEnum.PENDING,
                    DepositId = deposit.Data.Id
                };

                var transaction = await _transactionRepository.CreateTransactionAsync(newTransaction);
                if (transaction.IsSuccess == false || transaction.Data == null)
                {
                    scope.Dispose();
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                scope.Complete();
                return new Return<dynamic>
                {
                    Data = new { paymentUrl },
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {

                scope.Dispose();
                return new Return<dynamic>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<bool>> CallbackVnPayIPNUrl(string vnp_TmnCode, string vnp_Amount, string vnp_BankCode, string vnp_OrderInfo, string vnp_TransactionNo, string vnp_ResponseCode, string vnp_TransactionStatus, Guid vnp_TxnRef, string vnp_SecureHash)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var deposit = await _depositRepository.GetDepositByIdAsync(vnp_TxnRef);
                if (!deposit.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || deposit.Data == null)
                {
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                var package = await _packageRepository.GetPackageByPackageIdAsync(deposit.Data.PackageId);
                if (!package.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || package.Data == null)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                var transaction = await _transactionRepository.GetTransactionByDepositIdAsync(deposit.Data.Id);
                if (!transaction.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || transaction.Data == null)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                var vnpay = new VnPayLibrary();

                // checksum
                string vnp_HashSecret = _configuration.GetSection("VnPay:vnp_HashSecret").Value ?? "";
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (!checkSignature)
                {
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                if(transaction.Data.Amount != int.Parse(vnp_Amount))
                {
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                // update transaction 
                transaction.Data.TransactionStatus = (vnp_TransactionStatus.Equals("00") && vnp_TransactionStatus == "00")  ? StatusTransactionEnum.SUCCEED : StatusTransactionEnum.FAILED;
                var updateTransaction = await _transactionRepository.UpdateTransactionAsync(transaction.Data);
                if (updateTransaction != null) {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                // update deposit appTranId
                var updateDeposit = await _depositRepository.UpdateDepositAppTranIdAsync(deposit.Data.Id, vnp_TransactionNo);
                if (updateDeposit != null) {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }


                // wallet
                var walletMain = await _walletRepository.GetMainWalletByCustomerId(deposit.Data.CustomerId);
                if (!walletMain.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || walletMain.Data == null)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                walletMain.Data.Balance += package.Data.CoinAmount;

                // Create transaction for wallet main
                FUParkingModel.Object.Transaction newTransactionMain = new()
                {
                    Amount = package.Data.CoinAmount,
                    TransactionDescription = "Buy " + package.Data.Name,
                    TransactionStatus = StatusTransactionEnum.SUCCEED,
                    WalletId = walletMain.Data.Id
                };
                var transactionMain = await _transactionRepository.CreateTransactionAsync(newTransactionMain);
                if (transactionMain.IsSuccess == false || transactionMain.Data == null)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var updateWalletMain = await _walletRepository.UpdateWalletAsync(walletMain.Data);
                if (!updateWalletMain.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                var walletExtra = await _walletRepository.GetExtraWalletByCustomerId(deposit.Data.CustomerId);
                if (!walletExtra.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || walletExtra.Data == null)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                walletExtra.Data.Balance += package.Data.ExtraCoin ?? 0;
                walletExtra.Data.EXPDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).AddDays(package.Data.EXPPackage ?? 0);
                // Create transaction for wallet extra
                if (package.Data.ExtraCoin is not null)
                {
                    FUParkingModel.Object.Transaction newTransactionExtra = new()
                    {
                        Amount = package.Data.ExtraCoin ?? 0,
                        TransactionDescription = "Buy " + package.Data.Name,
                        TransactionStatus = StatusTransactionEnum.SUCCEED,
                        WalletId = walletExtra.Data.Id
                    };
                    var transactionExtra = await _transactionRepository.CreateTransactionAsync(newTransactionExtra);
                    if (transactionExtra.IsSuccess == false || transactionExtra.Data == null)
                    {
                        scope.Dispose();
                        return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                    }
                }
                var updateWalletExtra = await _walletRepository.UpdateWalletAsync(walletExtra.Data);
                if (!updateWalletExtra.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                scope.Complete();
                return new Return<bool> { Message = SuccessfullyEnumServer.SUCCESSFULLY, IsSuccess = true };

            }
            catch (Exception ex)
            {
                return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }
    }
}
