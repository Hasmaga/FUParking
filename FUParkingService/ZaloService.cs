using FUParkingModel.Enum;
using FUParkingModel.Object;
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
        private readonly ICustomerService _customerService;
        private readonly IHelpperService _helperService;
        private readonly IPackageRepository _packageRepository;        
        private readonly IConfiguration _configuration;
        private readonly IWalletRepository _walletRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IDepositRepository _depositRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHttpClientFactory _httpClient;

        public ZaloService(ICustomerService customerService, IHelpperService helperService, IPackageRepository packageRepository, IConfiguration configuration, IWalletRepository walletRepository, IPaymentRepository paymentRepository, IDepositRepository depositRepository, ITransactionRepository transactionRepository, IHttpClientFactory httpClient)
        {
            _customerService = customerService;
            _helperService = helperService;
            _packageRepository = packageRepository;
            _configuration = configuration;
            _walletRepository = walletRepository;
            _paymentRepository = paymentRepository;
            _depositRepository = depositRepository;
            _transactionRepository = transactionRepository;
            _httpClient = httpClient;
        }

        public async Task<Return<bool>> CustomerCreateRequestBuyPackageByZaloPayAsync(Guid packageId)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                if (!_helperService.IsTokenValid())
                {
                    return new Return<bool> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                var userLogged = await _customerService.GetCustomerByIdAsync(_helperService.GetAccIdFromLogged());
                if (userLogged.IsSuccess == false || userLogged.Data == null)
                {
                    return new Return<bool> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                var package = await _packageRepository.GetPackageByPackageIdAsync(packageId);
                if (package.IsSuccess == false || package.Data == null)
                {
                    return new Return<bool> { Message = ErrorEnumApplication.PACKAGE_NOT_EXIST };
                }
                // Call Zalo API to create request buy package
                string appid = _configuration.GetSection("AppSettings:ZaloPay:AppId").Value ?? "";
                string key1 = _configuration.GetSection("AppSettings:ZaloPay:Key1").Value ?? "";
                string createOrderUrl = _configuration.GetSection("AppSettings:ZaloPay:CreateOrderUrl").Value ?? "";
                string callbackUrl = _configuration.GetSection("AppSettings:ZaloPay:CallbackUrl").Value ?? "";
                string bankCode = _configuration.GetSection("AppSettings:ZaloPay:BankCode").Value ?? "";
                if (string.IsNullOrEmpty(appid) || string.IsNullOrEmpty(key1) || string.IsNullOrEmpty(createOrderUrl) || string.IsNullOrEmpty(callbackUrl) || string.IsNullOrEmpty(bankCode))
                {
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                Random rnd = new();
                var embed_data = new
                {
                    redirecturl = callbackUrl,
                    accountId = userLogged.Data.Id,
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
                var param = new Dictionary<string, string>()
                {
                    { "app_id", appid },
                    { "app_user", "Bai Parking FPT" },
                    { "app_time", GetTimeStamp().ToString() },
                    { "amount", package.Data.Price.ToString() },
                    { "app_tran_id", DateTime.Now.ToString("yyMMdd") + "_" + rnd.Next(1000000) },
                    { "embed_data", JsonConvert.SerializeObject(embed_data) },
                    { "item", JsonConvert.SerializeObject(items) },
                    { "description", "Mua gói dịch vụ " + package.Data.Name + " Bai Parking FPT" },
                    { "bank_code", bankCode },
                    { "email", "baiparking@gmail.com" }
                };
                var data = appid + "|" + param["app_trans_id"] + "|" + param["app_user"] + "|" + param["amount"] + "|" + param["app_time"] + "|" + param["embed_data"] + "|" + param["item"];
                param.Add("mac", Compute(ZaloPayHMAC.HMACSHA256, key1, data));
                var response = await PostFormAsync(createOrderUrl, param);
                string result = JsonConvert.SerializeObject(response);
                // Create transaction
                var walletData = (await _walletRepository.GetWalletByCustomerId(userLogged.Data.Id)).Data;
                var paymentMethod = (await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.ZALOPAY)).Data;
                if (walletData == null || paymentMethod == null)
                {
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                // Create Deposit
                Deposit newDeposit = new()
                {
                    PaymentMethodId = paymentMethod.Id,
                    PackageId = package.Data.Id,
                    CustomerId = walletData.CustomerId,
                    Amount = package.Data.Price,
                    AppTranId = param["app_tran_id"]
                };
                var deposit = await _depositRepository.CreateDepositAsync(newDeposit);
                if (deposit.IsSuccess == false || deposit.Data == null)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                FUParkingModel.Object.Transaction newTransaction = new()
                {
                    WalletId = walletData.CustomerId,
                    Amount = package.Data.Price,
                    TransactionDescription = "UserId: " + userLogged.Data.Id + " buy " + package.Data.Name,
                    TransactionStatus = StatusTransactionEnum.PENDING,
                    DepositId = deposit.Data.Id
                };
                var transaction = await _transactionRepository.CreateTransactionAsync(newTransaction);
                if (transaction.IsSuccess == false || transaction.Data == null)
                {
                    scope.Dispose();
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                scope.Complete();
                return new Return<bool> { Data = true, Message = SuccessfullyEnumServer.SUCCESSFULLY };
            }
            catch (Exception ex)
            {
                scope.Dispose();
                return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex.Message };
            }
        }

        #region Private Method
        private static long GetTimeStamp(DateTime date)
        {
            return (long)(date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        private static long GetTimeStamp()
        {
            return GetTimeStamp(DateTime.Now);
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
            return JsonConvert.DeserializeObject<T>(responseString);
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
            return JsonConvert.DeserializeObject<T>(responseString);
        }

        public Task<Dictionary<string, object>> GetJson(string uri)
        {
            return GetJson<Dictionary<string, object>>(uri);
        }


        #endregion
    }
}
