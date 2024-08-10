using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Statistic
{
    public class StatisticPaymentByCustomerResDto
    {
        public DateTime Date { get; set; }
        public int TotalPayment { get; set; }
        public int Amount { get; set; }
    }
}
