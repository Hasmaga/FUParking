using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Wallet
{
    public class GetWalletExtraResDto
    {
        public int Balance { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }
}
