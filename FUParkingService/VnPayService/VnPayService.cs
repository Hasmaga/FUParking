using FirebaseService;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Firebase;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Helper;
using FUParkingService.Interface;
using FUParkingService.MailObject;
using FUParkingService.MailService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
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
        private readonly IFirebaseService _firebaseService;
        private readonly IMailService _mailService;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<VnPayService> _logger;

        public VnPayService(
            IPackageRepository packageRepository,
            IConfiguration configuration,
            IHelpperService helperService,
            IPaymentRepository paymentRepository,
            IDepositRepository depositRepository,
            ITransactionRepository transactionRepository,
            IWalletRepository walletRepository,
            IFirebaseService firebaseService,
            IMailService mailService,
            ICustomerRepository customerRepository,
            ILogger<VnPayService> logger)
        {
            _configuration = configuration;
            _packageRepository = packageRepository;
            _helperService = helperService;
            _paymentRepository = paymentRepository;
            _depositRepository = depositRepository;
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _firebaseService = firebaseService;
            _mailService = mailService;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task<Return<dynamic>> CustomerCreateRequestBuyPackageByVnPayAsync(Guid packageId, string vnp_BankCode, IPAddress ipAddress)
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
                vnpay.AddRequestData("vnp_BankCode", vnp_BankCode);
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

        public async Task<Return<bool>> CallbackVnPayIPNUrl(IQueryCollection queryStringParameters)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                VnPayLibrary vnpay = new();
                foreach (var key in queryStringParameters.Keys)
                {
                    var value = queryStringParameters[key];
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && !string.IsNullOrEmpty(value))
                    {
#pragma warning disable CS8604 // Possible null reference argument.
                        vnpay.AddResponseData(key, value);
#pragma warning restore CS8604 // Possible null reference argument.
                    }
                }

                Guid depositId = Guid.Parse(vnpay.GetResponseData("vnp_TxnRef"));
                int vnp_Amount = int.Parse(vnpay.GetResponseData("vnp_Amount")) / 100;
                string vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                string vnp_CardType = vnpay.GetResponseData("vnp_CardType");
                string? vnp_SecureHash = queryStringParameters["vnp_SecureHash"];

                if (vnp_SecureHash is null)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                string vnp_HashSecret = _configuration.GetSection("VnPay:vnp_HashSecret").Value ?? "";
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (!checkSignature)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                var deposit = await _depositRepository.GetDepositByIdAsync(depositId);
                if (!deposit.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || deposit.Data == null)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                if (deposit.Data.Amount != vnp_Amount)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                if (vnp_TransactionStatus.Equals("00") == false)
                {
                    scope.Dispose();
                    _logger.LogError("Transaction failed with status: {Status}", vnp_TransactionStatus);
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                var package = await _packageRepository.GetPackageByPackageIdAsync(deposit.Data.PackageId);
                if (!package.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || package.Data == null)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                var updateDeposit = await _depositRepository.UpdateDepositAppTranIdAsync(deposit.Data.Id, vnpayTranId);
                if (updateDeposit == null)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                int mainAmount = 0;
                int extraAmount = 0;
                DateTime? expDate = null;

                var walletMain = await _walletRepository.GetMainWalletByCustomerId(deposit.Data.CustomerId);
                if (!walletMain.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || walletMain.Data == null)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                walletMain.Data.Balance += package.Data.CoinAmount;
                mainAmount = walletMain.Data.Balance;

                FUParkingModel.Object.Transaction newTransactionMain = new()
                {
                    Amount = package.Data.CoinAmount,
                    DepositId = deposit.Data.Id,
                    TransactionDescription = "Buy " + package.Data.Name,
                    TransactionStatus = StatusTransactionEnum.SUCCEED,
                    WalletId = walletMain.Data.Id
                };
                var transactionMain = await _transactionRepository.CreateTransactionAsync(newTransactionMain);
                if (!transactionMain.IsSuccess || transactionMain.Data == null)
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

                var currentDateTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                if (package.Data.ExtraCoin > 1)
                {
                    var walletExtra = await _walletRepository.GetExtraWalletByCustomerId(deposit.Data.CustomerId);
                    if (!walletExtra.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || walletExtra.Data == null)
                    {
                        scope.Dispose();
                        return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                    }
                    walletExtra.Data.Balance += package.Data.ExtraCoin ?? 0;

                    walletExtra.Data.EXPDate = (walletExtra.Data.EXPDate?.Date < currentDateTime.Date)
                            ? currentDateTime.AddDays(package.Data.EXPPackage ?? 0)
                            : walletExtra.Data.EXPDate?.AddDays(package.Data.EXPPackage ?? 0);

                    extraAmount = walletExtra.Data.Balance;
                    expDate = walletExtra.Data.EXPDate;

                    // Create transaction for wallet extra
                    if (package.Data.ExtraCoin is not null)
                    {
                        FUParkingModel.Object.Transaction newTransactionExtra = new()
                        {
                            Amount = package.Data.ExtraCoin ?? 0,
                            DepositId = deposit.Data.Id,
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
                }

                //Notification
                var customer = await _customerRepository.GetCustomerByIdAsync(deposit.Data.CustomerId);
                if (customer.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) && customer.Data is not null)
                {
                    var title = "Top-up Successful!";
                    var body = $"You have successfully topped up {Utilities.FormatMoney(package.Data.CoinAmount)} bic into your main wallet via {vnp_CardType} using VnPay Payment Gateway.\n\n" +
                        $"Please log in to the Bai app to see more. If you have any questions, please contact Bai Parking directly or visit the Support section in the Bai app.";

                    // Send Firebase notification
                    var fcmToken = deposit.Data.Customer?.FCMToken;
                    if (!string.IsNullOrEmpty(fcmToken))
                    {

                        var firebaseReq = new FirebaseReqDto
                        {
                            ClientTokens = new List<string> { fcmToken },
                            Title = title,
                            Body = body
                        };

                        var notificationResult = await _firebaseService.SendNotificationAsync(firebaseReq);

                        if (!notificationResult.IsSuccess)
                        {
                            _logger.LogError("Failed to send notification to customer Id {CustomerId}. Error: {Error}", deposit.Data.CustomerId, notificationResult.InternalErrorMessage);
                        }
                    }

                    MailRequest mailRequest = new()
                    {
                        ToEmail = deposit.Data.Customer?.Email ?? "",
                        ToUsername = deposit.Data.Customer?.FullName ?? "",
                        Subject = title,
                        Body = body
                    };

                    // Send Mail
                    await _mailService.SendEmailAsync(mailRequest);
                }
                else
                {
                    _logger.LogError("Failed to retrieve customer with ID {CustomerId}. Error: {ErrorMessage}", deposit.Data.CustomerId, customer.Message);
                }

                scope.Complete();
                return new Return<bool> { Data = true, Message = SuccessfullyEnumServer.SUCCESSFULLY, IsSuccess = true };

            }
            catch (Exception ex)
            {
                scope.Dispose();
                return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }
    }
}
