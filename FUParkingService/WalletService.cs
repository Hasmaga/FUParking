using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ICustomerRepository _customerRepository;

        public WalletService(IWalletRepository walletRepository, ICustomerRepository customerRepository)
        {
            _walletRepository = walletRepository;
            _customerRepository = customerRepository;
        }

        public async Task<Return<List<Transaction>>> GetWalletTransactionByCustomerIdAsync(string? customerId, int pageIndex, int pageSize, int numberOfDays)
        {
            Return<List<Transaction>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            DateTime now = DateTime.Now;
            DateTime lastDate = now.AddDays(-numberOfDays);

            try
            {
                if (customerId == null)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                Guid customerGuid = new(customerId);
                Return<Customer> customerRes = await _customerRepository.GetCustomerByIdAsync(customerGuid);
                if (customerRes.Data == null)
                {
                    res.Message = customerRes.Message;
                    return res;
                }

                Return<Wallet> foundWalletRes = await _walletRepository.GetWalletByCustomerId(customerGuid);
                if (foundWalletRes.Data == null)
                {
                    res.Message = foundWalletRes.Message;
                    return res;
                }
                Return<IEnumerable<Transaction>> transactionRes = await _walletRepository.GetWalletTransactionByWalletIdAsync(foundWalletRes.Data.Id, pageIndex, pageSize, now, lastDate);
                if (transactionRes.Data == null && !transactionRes.IsSuccess)
                {
                    res.Message = transactionRes.Message;
                    return res;
                }
                res.Message = transactionRes.Message;
                res.IsSuccess = true;
                res.Data = transactionRes.Data.ToList();
                return res;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
