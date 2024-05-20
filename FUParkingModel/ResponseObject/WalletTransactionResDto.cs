using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FUParkingModel.ResponseObject
{
    public class WalletTransactionResDto
    {
        public IEnumerable<Transaction>? listTransactions {  get; set; }

    }
}
