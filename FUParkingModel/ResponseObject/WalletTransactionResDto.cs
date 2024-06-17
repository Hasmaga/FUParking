using System.Transactions;

namespace FUParkingModel.ResponseObject
{
    public class WalletTransactionResDto
    {
        public IEnumerable<Transaction>? ListTransactions { get; set; }

    }
}
