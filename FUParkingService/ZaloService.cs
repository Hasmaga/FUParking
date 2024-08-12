using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Zalo;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Transactions;

namespace FUParkingService
{
    public class ZaloService : IZaloService
    {
        private readonly IHelpperService _helperService;
        private readonly IPackageRepository _packageRepository;
        private readonly IConfiguration _configuration;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IDepositRepository _depositRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHttpClientFactory _httpClient;
        private readonly IWalletRepository _walletRepository;

        public ZaloService(IHelpperService helperService, IPackageRepository packageRepository, IConfiguration configuration, IPaymentRepository paymentRepository, IDepositRepository depositRepository, ITransactionRepository transactionRepository, IHttpClientFactory httpClient, IWalletRepository walletRepository)
        {
            _helperService = helperService;
            _packageRepository = packageRepository;
            _configuration = configuration;
            _paymentRepository = paymentRepository;
            _depositRepository = depositRepository;
            _transactionRepository = transactionRepository;
            _httpClient = httpClient;
            _walletRepository = walletRepository;
        }

        public async Task<Return<ZaloResDto>> CustomerCreateRequestBuyPackageByZaloPayAsync(Guid packageId)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var checkAuth = await _helperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<ZaloResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var package = await _packageRepository.GetPackageByPackageIdAsync(packageId);
                if (package.IsSuccess == false || package.Data == null)
                {
                    return new Return<ZaloResDto> { Message = ErrorEnumApplication.PACKAGE_NOT_EXIST };
                }
                // Call Zalo API to create request buy package
                string appid = _configuration.GetSection("ZaloPay:AppId").Value ?? "";
                string key1 = _configuration.GetSection("ZaloPay:Key1").Value ?? "";
                string createOrderUrl = _configuration.GetSection("ZaloPay:CreateOrderUrl").Value ?? "";
                string redirectUrl = _configuration.GetSection("ZaloPay:RedirectUrl").Value ?? "";
                string bankCode = _configuration.GetSection("ZaloPay:BankCode").Value ?? "";
                if (string.IsNullOrEmpty(appid) || string.IsNullOrEmpty(key1) || string.IsNullOrEmpty(createOrderUrl) || string.IsNullOrEmpty(redirectUrl) || string.IsNullOrEmpty(bankCode))
                {
                    return new Return<ZaloResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                Random rnd = new();
                var embed_data = new
                {
                    redirecturl = redirectUrl,
                    accountId = checkAuth.Data.Id.ToString()
                };
                var items = new[]
                {
                    new
                    {
                        itemid = package.Data.Id,
                        itemname = package.Data.Name,
                        itemprice = package.Data.Price,
                        itemquantity = 1,
                    }
                };

                string tran;
                bool isTranExist;
                do
                {
                    tran = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyMMdd") + "_" + rnd.Next(1000000);
                    // Check if tran exists in the database
                    var depositExist = await _depositRepository.GetDepositByAppTransIdAsync(tran);
                    if (depositExist.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                    {
                        isTranExist = false;
                    }
                    else
                    {
                        isTranExist = true;
                    }
                } while (isTranExist);

                var param = new Dictionary<string, string>()
                {
                    { "app_id", appid },
                    { "app_user", "Bai Parking FPT" },
                    { "app_time", GetTimeStamp().ToString() },
                    { "amount", package.Data.Price.ToString() },
                    { "app_trans_id", tran },
                    { "embed_data", JsonConvert.SerializeObject(embed_data) },
                    { "item", JsonConvert.SerializeObject(items) },
                    { "description", "Mua gói dịch vụ " + package.Data.Name + " Bai Parking FPT" },
                    { "bank_code", bankCode },
                    { "callback_url", "https://backend.khangbpa.com/callback"},
                    { "email", "khangbpak2001@gmail.com" }
                };
                var data = appid + "|" + param["app_trans_id"] + "|" + param["app_user"] + "|" + param["amount"] + "|" + param["app_time"] + "|" + param["embed_data"] + "|" + param["item"];
                param.Add("mac", Compute(ZaloPayHMAC.HMACSHA256, key1, data));
                var response = await PostFormAsync(createOrderUrl, param);
                string result = JsonConvert.SerializeObject(response);
                // Convert result to ZaloPayResDto object
                var zaloPayResDto = JsonConvert.DeserializeObject<ZaloResDto>(result);
                if (zaloPayResDto?.Return_code != 1)
                {
                    return new Return<ZaloResDto> { Message = ErrorEnumApplication.SERVER_ERROR, Data = zaloPayResDto };
                }
                // Create transaction                
                var paymentMethod = (await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.ZALOPAY)).Data;
                if (paymentMethod == null)
                {
                    return new Return<ZaloResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                // Create Deposit
                Deposit newDeposit = new()
                {
                    PaymentMethodId = paymentMethod.Id,
                    PackageId = package.Data.Id,
                    CustomerId = checkAuth.Data.Id,
                    Amount = package.Data.Price,
                    AppTranId = tran
                };
                var deposit = await _depositRepository.CreateDepositAsync(newDeposit);
                if (deposit.IsSuccess == false || deposit.Data == null)
                {
                    scope.Dispose();
                    return new Return<ZaloResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                FUParkingModel.Object.Transaction newTransaction = new()
                {
                    Amount = package.Data.Price,
                    TransactionDescription = "UserId: " + checkAuth.Data.Id + " buy " + package.Data.Name,
                    TransactionStatus = StatusTransactionEnum.PENDING,
                    DepositId = deposit.Data.Id
                };
                var transaction = await _transactionRepository.CreateTransactionAsync(newTransaction);
                if (transaction.IsSuccess == false || transaction.Data == null)
                {
                    scope.Dispose();
                    return new Return<ZaloResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                scope.Complete();
                return new Return<ZaloResDto> { Data = zaloPayResDto, Message = SuccessfullyEnumServer.SUCCESSFULLY, IsSuccess = true };
            }
            catch (Exception ex)
            {
                scope.Dispose();
                return new Return<ZaloResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }


        public async Task<Return<bool>> CallbackZaloPayAsync(string app_trans_id)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var deposit = await _depositRepository.GetDepositByAppTransIdAsync(app_trans_id);
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
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                transaction.Data.TransactionStatus = StatusTransactionEnum.SUCCEED;
                var updateTransaction = await _transactionRepository.UpdateTransactionAsync(transaction.Data);
                if (!updateTransaction.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }                
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

        #region Private Method
        private static long GetTimeStamp(DateTime date)
        {
            return (long)(date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        private static long GetTimeStamp()
        {
            return GetTimeStamp(TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")));
        }

        private enum ZaloPayHMAC
        {
            HMACMD5,
            HMACSHA1,
            HMACSHA256,
            HMACSHA512
        }
        private static string Compute(ZaloPayHMAC algorithm = ZaloPayHMAC.HMACSHA256, string key = "", string message = "")
        {
            byte[] keyByte = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
            byte[] hashMessage = algorithm switch
            {
                ZaloPayHMAC.HMACMD5 => new HMACMD5(keyByte).ComputeHash(messageBytes),
                ZaloPayHMAC.HMACSHA1 => new HMACSHA1(keyByte).ComputeHash(messageBytes),
                ZaloPayHMAC.HMACSHA256 => new HMACSHA256(keyByte).ComputeHash(messageBytes),
                ZaloPayHMAC.HMACSHA512 => new HMACSHA512(keyByte).ComputeHash(messageBytes),
                _ => new HMACSHA256(keyByte).ComputeHash(messageBytes),
            };
            return BitConverter.ToString(hashMessage).Replace("-", "").ToLower();
        }
        private async Task<T> PostAsync<T>(string uri, HttpContent content)
        {
            var response = await _httpClient.CreateClient().PostAsync(uri, content);
            var responseString = await response.Content.ReadAsStringAsync();
#pragma warning disable CS8603 // Possible null reference return.
            return JsonConvert.DeserializeObject<T>(responseString);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public Task<Dictionary<string, object>> PostAsync(string uri, HttpContent content)
        {
            return PostAsync<Dictionary<string, object>>(uri, content);
        }

        public Task<T> PostFormAsync<T>(string uri, Dictionary<string, string> data)
        {
            return PostAsync<T>(uri, new FormUrlEncodedContent(data));
        }

        public Task<Dictionary<string, object>> PostFormAsync(string uri, Dictionary<string, string> data)
        {
            return PostFormAsync<Dictionary<string, object>>(uri, data);
        }

        public async Task<T> GetJson<T>(string uri)
        {
            var response = await _httpClient.CreateClient().GetAsync(uri);
            var responseString = await response.Content.ReadAsStringAsync();
#pragma warning disable CS8603 // Possible null reference return.
            return JsonConvert.DeserializeObject<T>(responseString);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public Task<Dictionary<string, object>> GetJson(string uri)
        {
            return GetJson<Dictionary<string, object>>(uri);
        }


        #endregion
    }
}
