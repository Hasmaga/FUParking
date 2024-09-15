using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Wallet
{
    public class GetBalanceWalletMainExtraResDto
    {
        public int BalanceMain { get; set; }
        public int BalanceExtra { get; set; }
        public DateTime? ExpDate { get; set; }
    }
}
