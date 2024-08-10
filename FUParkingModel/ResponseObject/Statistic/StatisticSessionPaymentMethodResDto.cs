using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Statistic
{
    public class StatisticSessionPaymentMethodResDto
    {
        public string PaymentMethod { get; set; } = null!;
        public int TotalPayment { get; set; }
    }
}
